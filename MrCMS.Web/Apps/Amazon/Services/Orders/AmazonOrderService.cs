﻿using MrCMS.Helpers;
using MrCMS.Paging;
using MrCMS.Web.Apps.Amazon.Entities.Orders;
using MrCMS.Web.Apps.Amazon.Models;
using MrCMS.Web.Apps.Amazon.Services.Logs;
using NHibernate;
using NHibernate.Criterion;

namespace MrCMS.Web.Apps.Amazon.Services.Orders
{
    public class AmazonOrderService : IAmazonOrderService
    {
        private readonly ISession _session;
        private readonly IAmazonLogService _amazonLogService;

        public AmazonOrderService(ISession session, IAmazonLogService amazonLogService)
        {
            _session = session;
            _amazonLogService = amazonLogService;
        }

        public AmazonOrder Get(int id)
        {
            return _session.QueryOver<AmazonOrder>()
                            .Where(item => item.Id == id).SingleOrDefault();
        }

        public AmazonOrder GetByAmazonOrderId(string id)
        {
            return _session.QueryOver<AmazonOrder>()
                            .Where(item => item.AmazonOrderId.IsInsensitiveLike(id,MatchMode.Exact)).SingleOrDefault();
        }

        public IPagedList<AmazonOrder> Search(string queryTerm = null, int page = 1, int pageSize = 10)
        {
            if (!string.IsNullOrWhiteSpace(queryTerm))
            {
                return _session.QueryOver<AmazonOrder>()
                                    .Where(x => 
                                        x.BuyerEmail.IsInsensitiveLike(queryTerm, MatchMode.Anywhere) 
                                        || x.BuyerName.IsInsensitiveLike(queryTerm, MatchMode.Anywhere)
                                        || x.AmazonOrderId.IsInsensitiveLike(queryTerm, MatchMode.Anywhere)
                                        ).Paged(page, pageSize);
            }

            return _session.Paged(QueryOver.Of<AmazonOrder>(), page, pageSize);
        }

        public void Save(AmazonOrder item)
        {
            _amazonLogService.Add(AmazonLogType.Orders, item.Id > 0 ? AmazonLogStatus.Update : AmazonLogStatus.Insert,
                                 null, null, null,null, item, null, null);

            _session.Transact(session => session.SaveOrUpdate(item));
        }

        public void Delete(AmazonOrder item)
        {
            _amazonLogService.Add(AmazonLogType.Listings, AmazonLogStatus.Delete, null,null,null, null, item, null,null);

            _session.Transact(session => session.Delete(item));
        }

        public void MarkAsShipped(AmazonOrder item)
        {
            item.Status = AmazonOrderStatus.Shipped;
            foreach (var orderItem in item.Items)
            {
                orderItem.QuantityShipped = orderItem.QuantityOrdered;
            }
            Save(item);
        }
    }
}