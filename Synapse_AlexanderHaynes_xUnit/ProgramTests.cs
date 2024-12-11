using Newtonsoft.Json.Linq;

namespace Synapse.OrdersExample.Tests {
    public class ProgramTests
    {
        /// <summary>
        /// The retrieval of medical equipment orders should be successful.
        /// </summary>
        [Fact]
        public async Task Positive_GetMedicalEquipmentOrders()
        {
            // Arrange
            var expectedOrderCount = 2;

            // Act
            var orders = await Program.FetchMedicalEquipmentOrdersAsync();

            // Assert
            Assert.NotNull(orders);
            Assert.Equal(expectedOrderCount, orders.Length);
        }

        /// <summary>
        /// An order item that does have a Status of 'Delivered' should incremement
        /// its deliveryNotification field.
        /// </summary>
        [Fact]
        public async Task Positive_ShouldIncrementDeliveryNotification()
        {
            // Arrange
            var order = new JObject
            {
                { "OrderId", 1 },
                { "Items", new JArray
                    {
                        new JObject
                        {
                            { "Status", "Delivered" },
                            { "Description", "X-Ray machine" },
                            { "deliveryNotification", 0 }
                        }
                    }
                }
            };

            // Act
            var processedOrder = await Program.ProcessOrderAsync(order);

            // Assert
            var item = processedOrder["Items"]![0];
            Assert.Equal(1, item!["deliveryNotification"]!.Value<int>());
        }

        /// <summary>
        /// An order item that doesn't have a Status of 'Delivered' should not incremement
        /// its deliveryNotification field.
        /// </summary>
        [Fact]
        public async Task Positive_ShouldNotIncrementDeliveryNotification()
        {
            // Arrange
            var order = new JObject
            {
                { "OrderId", 1 },
                { "Items", new JArray
                    {
                        new JObject
                        {
                            { "Status", "Ready_to_Deliver" },
                            { "Description", "LHZ 300 Kit" },
                            { "deliveryNotification", 0 }
                        }
                    }
                }
            };

            // Act
            var processedOrder = await Program.ProcessOrderAsync(order);

            // Assert
            var item = processedOrder["Items"]![0];
            Assert.Equal(0, item!["deliveryNotification"]!.Value<int>());
        }

        /// <summary>
        /// Verifies if item with Status of 'Delivered" returns True.
        /// </summary>
        [Fact]
        public void Positive_IsItemDelivered_ReturnsTrueForDeliveredItem()
        {
            // Arrange
            var item = new JObject
            {
                { "Status", "Delivered" }
            };

            // Act
            var result = Program.IsItemDelivered(item);

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Verifies if item with incorrect JSON syntax of "status" instead of "Status"
        /// triggers the 'catch' block of the try-catch and returns False.
        /// </summary>
        [Fact]
        public void Negative_IsItemDelivered_ReturnsFalseForBadJSONSyntax()
        {
            // Arrange
            var item = new JObject
            {
                { "status", "Delivered" }
            };

            // Act
            var result = Program.IsItemDelivered(item);

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Verifies if item with Status of 'Ready_to_Deliver" returns False.
        /// </summary>
        [Fact]
        public void Positive_IsItemDelivered_ReturnsFalseForNonDeliveredItem()
        {
            // Arrange
            var item = new JObject
            {
                { "Status", "Ready_to_Deliver" }
            };

            // Act
            var result = Program.IsItemDelivered(item);

            // Assert
            Assert.False(result);
        }
    }
}

