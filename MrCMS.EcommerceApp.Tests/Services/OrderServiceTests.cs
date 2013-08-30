﻿using FakeItEasy;
using MrCMS.Web.Apps.Ecommerce.Entities.Orders;
using MrCMS.Web.Apps.Ecommerce.Models;
using NHibernate;
using Xunit;
using MrCMS.Web.Apps.Ecommerce.Services.Orders;
using System;

namespace MrCMS.EcommerceApp.Tests.Services
{
    public class OrderServiceTests : MrCMSTest
    {
        private readonly ISession _session;
        private readonly OrderService _orderService;
        private readonly IOrderEventService _orderEventService;

        public OrderServiceTests()
        {
            _session = A.Fake<ISession>();
            _orderEventService = A.Fake<IOrderEventService>();
            _orderService = new OrderService(_session, _orderEventService);
        }

        //TODO PlaceOrder

        [Fact]
        public void OrderService_Cancel_ShouldCallOrderCancelled()
        {
            var order = new Order() { IsCancelled = true };

            _orderService.Cancel(order);

            A.CallTo(() => _orderEventService.OrderCancelled(order)).MustHaveHappened();
        }

        [Fact]
        public void OrderService_MarkAsShipped_ShouldCallOrderShipped()
        {
            var order = new Order() { ShippingStatus = ShippingStatus.Shipped, ShippingDate = DateTime.UtcNow };

            _orderService.MarkAsShipped(order);

            A.CallTo(() => _orderEventService.OrderShipped(order)).MustHaveHappened();
        }
    }
}