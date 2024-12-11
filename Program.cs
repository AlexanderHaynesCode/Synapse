using Newtonsoft.Json.Linq;
using RichardSzalay.MockHttp;

namespace Synapse.OrdersExample
{
    /// <summary>
    /// Retrieves a list of orders from the API DB.
    /// Orders with items whose Status is marked as Delivered have alerts sent out, deliveryNotification incremented, 
    /// and are updated back to the API DB.
    /// </summary>
    public class Program
    {
        public static readonly HttpClient httpClient = new(); // This would be used for the actual API connections
        public static readonly MockHttpMessageHandler mockHttp = new MockHttpMessageHandler(); // Used to mock data

        public static readonly LoggerClass logger = new();


        public static async Task<int> Main(string[] args)
        {
            logger.Write("Start of App");
            
            var medicalEquipmentOrders = await FetchMedicalEquipmentOrdersAsync();
            var orderLength = medicalEquipmentOrders.Length;
            var updatedOrders = new JObject[orderLength];
            for (int i = 0; i < orderLength; i++) {
                var updatedOrder = await ProcessOrderAsync(medicalEquipmentOrders[i]);
                updatedOrders[i] = updatedOrder;
            }

            if (updatedOrders.Length > 0) {
                // Moved the Update POST after the loop so all updated orders are posted at once, which is faster.
                await PostUpdatedOrdersAsync(updatedOrders);
            }            

            logger.Write("End of App");

            return 0;
        }

        /// <summary>
        /// Attempts to retrieve a list of medical equipment orders.
        /// </summary>
        public static async Task<JObject[]> FetchMedicalEquipmentOrdersAsync()
        {
            try
            {
                mockHttp.When("https://orders-api.com/orders")
                    .Respond("application/json", @"[{'OrderId': 1,'Items': [{'Status': 'Ready_to_Deliver','Description': 'LHZ 300 Kit','deliveryNotification': 0},{'Status': 'Delivered','Description': 'X-Ray machine','deliveryNotification': 0}]},
                    { 'OrderId': 2,'Items': [{ 'Status': 'Delivered','Description': 'X-Ray computer','deliveryNotification': 0},{ 'Status': 'Delivered','Description': 'New Chairs','deliveryNotification': 0}]}]");
                var httpClientMock = mockHttp.ToHttpClient();

                string ordersApiUrl = "https://orders-api.com/orders";
                var response = await httpClientMock.GetAsync(ordersApiUrl);
                if (response.IsSuccessStatusCode)
                {
                    var ordersData = await response.Content.ReadAsStringAsync();
                    var ordersJson = JArray.Parse(ordersData).ToObject<JObject[]>();

                    // The business wants to know if the order count is low or none, so log it.
                    if (ordersJson?.Length == 0) {
                        logger.Write("Zero orders found in FetchMedicalEquipmentOrdersAsync().");
                    }
                    
                    if (ordersJson != null) return ordersJson;
                    else return Array.Empty<JObject>();
                }
                return Array.Empty<JObject>();
            }
            catch (Exception e)
            {
                logger.Write($"Error in FetchMedicalEquipmentOrdersAsync(): {e}");
                return Array.Empty<JObject>();
            }
        }

        /// <summary>
        /// Processes an individual order, which includes checking for delivered items.
        /// Each delivered item triggers an alert and its delivery notification
        /// is incremented.
        /// </summary>
        /// <param name="order">The order being processed</param>
        public static async Task<JObject> ProcessOrderAsync(JObject order)
        {
            try {
                var items = order["Items"]!.ToObject<JArray>();
                foreach (var item in items!)
                {
                    if (IsItemDelivered(item))
                    {
                        await SendAlertMessageAsync(item, order["OrderId"]!.ToString());
                        // Increment the delivery notification
                        item["deliveryNotification"] = item["deliveryNotification"]!.Value<int>() + 1;
                    }
                }
                order["Items"] = items;

                return order;
            } catch (Exception e) {
                logger.Write($"Error in ProcessOrderAsync(): {e}");
                return order;
            }            
        }

        /// <summary>
        /// Returns 'true' if the order item is in a "Delivered" state, otherwise 'false.'
        /// </summary>
        /// <param name="item">The order item being checked for delivery status</param>
        public static bool IsItemDelivered(JToken item)
        {
            try {
                return item["Status"]!.ToString().Equals("Delivered", StringComparison.OrdinalIgnoreCase);
            } catch (Exception e) {
                logger.Write($"Error in IsItemDelivered(): {e}");
                return false;
            }            
        }

        /// <summary>
        /// Attempts to send an API Alert with the newly delivered order item.
        /// </summary>
        /// <param name="item">The order item for the alert</param>
        /// <param name="orderId">The order id for the alert</param>
        public static async Task SendAlertMessageAsync(JToken item, string orderId)
        {
            try
            {
                mockHttp.When("https://alert-api.com/alerts")
                    .Respond("application/json", "");
                var httpClientMock = mockHttp.ToHttpClient();

                string alertApiUrl = "https://alert-api.com/alerts";
                var alertData = new
                {
                    Message = $"Alert for delivered item: Order {orderId}, Item: {item["Description"]}, " +
                              $"Delivery Notifications: {item["deliveryNotification"]}"
                };
                var content = new StringContent(JObject.FromObject(alertData).ToString(), System.Text.Encoding.UTF8, "application/json");
                var response = await httpClientMock.PostAsync(alertApiUrl, content);

                if (response.IsSuccessStatusCode)
                {
                    logger.Write($"Alert sent for delivered item: {item["Description"]}");
                }
                else
                {
                    logger.Write($"Failed to send alert for delivered item: {item["Description"]}");
                }
            } catch (Exception e) {
                logger.Write($"Error in SendAlertMessageAsync(): {e}");
            }
        }

        /// <summary>
        /// Attempts to send an API Post with the newly updated orders.
        /// </summary>
        /// <param name="orders">The updated orders to be Posted to the API</param>
        public static async Task PostUpdatedOrdersAsync(JObject[] orders)
        {
            try
            {
                var ordersArray = new JArray(orders);

                mockHttp.When("https://update-api.com/update")
                    .Respond("application/json", "");
                var httpClientMock = mockHttp.ToHttpClient();

                string updateApiUrl = "https://update-api.com/update";
                var content = new StringContent(ordersArray.ToString(), System.Text.Encoding.UTF8, "application/json");
                var response = await httpClientMock.PostAsync(updateApiUrl, content);

                if (response.IsSuccessStatusCode)
                {
                    foreach (var order in orders) {
                        logger.Write($"Updated order sent for processing: OrderId {order["OrderId"]}");
                    }                    
                }
                else
                {
                    foreach (var order in orders) {
                        logger.Write($"Failed to send updated order for processing: OrderId {order["OrderId"]}");
                    }                        
                }
            } catch (Exception e) {
                logger.Write($"Error in PostUpdatedOrdersAsync(): {e}");
            }
        }
    }
}