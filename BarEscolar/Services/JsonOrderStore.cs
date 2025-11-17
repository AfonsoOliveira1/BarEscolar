using BarEscolar.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace BarEscolar.Services
{
    public class JsonOrderStore
    {
        private readonly string _ordersFile = "Data/orders.json";
        private readonly string _orderItemsFile = "Data/orderItems.json";

        private List<Order> _orders;
        private List<OrderItem> _orderItems;

        public JsonOrderStore()
        {
            _orders = LoadOrders();
            _orderItems = LoadOrderItems();
        }

        // ---------------- LOAD / SAVE ----------------
        private List<Order> LoadOrders()
        {
            if (!File.Exists(_ordersFile)) return new List<Order>();
            var json = File.ReadAllText(_ordersFile);
            return JsonSerializer.Deserialize<List<Order>>(json) ?? new List<Order>();
        }

        private List<OrderItem> LoadOrderItems()
        {
            if (!File.Exists(_orderItemsFile)) return new List<OrderItem>();
            var json = File.ReadAllText(_orderItemsFile);
            return JsonSerializer.Deserialize<List<OrderItem>>(json) ?? new List<OrderItem>();
        }

        private void SaveOrders() => File.WriteAllText(_ordersFile, JsonSerializer.Serialize(_orders, new JsonSerializerOptions { WriteIndented = true }));
        private void SaveOrderItems() => File.WriteAllText(_orderItemsFile, JsonSerializer.Serialize(_orderItems, new JsonSerializerOptions { WriteIndented = true }));

        // ---------------- ORDERS ----------------
        public List<Order> GetOrders() => _orders;

        public List<Order> GetOrdersByUser(string userId)
        {
            var userOrders = _orders.Where(o => o.Userid == userId).ToList();
            foreach (var order in userOrders)
            {
                order.OrderItems = _orderItems.Where(oi => oi.Orderid == order.Id).ToList();
            }
            return userOrders;
        }

        public Order GetOrderById(int orderId) => _orders.FirstOrDefault(o => o.Id == orderId);

        public void AddOrder(Order order)
        {
            _orders.Add(order);
            SaveOrders();
        }

        public void RemoveOrder(int orderId)
        {
            var order = GetOrderById(orderId);
            if (order != null)
            {
                // Remove associated order items
                _orderItems.RemoveAll(oi => oi.Orderid == orderId);
                _orders.Remove(order);
                SaveOrders();
                SaveOrderItems();
            }
        }

        public int GetNextOrderId() => _orders.Any() ? _orders.Max(o => o.Id) + 1 : 1;

        // ---------------- ORDER ITEMS ----------------
        public List<OrderItem> GetOrderItems() => _orderItems;

        public List<OrderItem> GetOrderItemsByUser(string userId)
        {
            var userOrders = GetOrdersByUser(userId).Select(o => o.Id).ToList();
            return _orderItems.Where(oi => userOrders.Contains(oi.Orderid)).ToList();
        }

        public List<OrderItem> GetOrderItemsByOrder(int orderId) => _orderItems.Where(oi => oi.Orderid == orderId).ToList();

        public OrderItem GetOrderItemById(int orderItemId) => _orderItems.FirstOrDefault(oi => oi.Id == orderItemId);

        public void AddOrderItem(OrderItem item)
        {
            _orderItems.Add(item);
            SaveOrderItems();
        }

        public void RemoveOrderItem(int orderItemId)
        {
            var item = GetOrderItemById(orderItemId);
            if (item != null)
            {
                _orderItems.Remove(item);

                // Remove order if empty
                var order = GetOrderById(item.Orderid);
                if (order != null && !_orderItems.Any(oi => oi.Orderid == order.Id))
                    _orders.Remove(order);

                SaveOrders();
                SaveOrderItems();
            }
        }

        public int GetNextOrderItemId() => _orderItems.Any() ? _orderItems.Max(oi => oi.Id) + 1 : 1;

        // ---------------- HELPER METHODS ----------------
        public void MarkMenu(string userId, int menuId)
        {
            var today = DateTime.Today;
            var order = _orders.FirstOrDefault(o => o.Userid == userId && o.Createdat.Date == today);

            if (order == null)
            {
                order = new Order
                {
                    Id = GetNextOrderId(),
                    Userid = userId,
                    Total = 0,
                    Createdat = DateTime.Now,
                    State = true,
                    OrderItems = new List<OrderItem>()
                };
                _orders.Add(order);
            }

            var orderItem = new OrderItem
            {
                Id = GetNextOrderItemId(),
                Orderid = order.Id,
                Productid = menuId,
                Quantity = 1,
                Unitprice = 0,
                Subtotal = 0
            };
            _orderItems.Add(orderItem);
            SaveOrders();
            SaveOrderItems();
        }

        public void CancelOrderItemById(int orderItemId) => RemoveOrderItem(orderItemId);

        public Dictionary<int, int> GetRemainingSeats()
        {
            return _orderItems
                .GroupBy(oi => oi.Productid)
                .ToDictionary(g => g.Key, g => g.Count());
        }

        public bool IsMenuAlreadyMarked(string userId, DateTime menuDate)
        {
            var userOrders = GetOrdersByUser(userId);
            var markedDates = userOrders.SelectMany(o => o.OrderItems).Select(oi => oi.Productid).ToList();
            return markedDates.Any();
        }
    }
}
