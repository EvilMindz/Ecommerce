﻿using System;
using System.Linq;
using MrCMS.Entities;
using MrCMS.Entities.Documents;
using MrCMS.Entities.Multisite;
using MrCMS.Entities.People;
using MrCMS.Helpers;
using MrCMS.Website;
using NHibernate;
using NHibernate.Event;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MrCMS.DbConfiguration.Configuration
{
    public class PostCommitEventListener : IPostUpdateEventListener
    {
        public void OnPostUpdate(PostUpdateEvent @event)
        {
            try
            {
                var session = @event.Session.SessionFactory.OpenFilteredSession();
                if (@event.Entity is Document)
                {
                    var ignorePropertyNames = new[]
                        {
                            "UpdatedOn", "Id", "CreatedOn"
                        };
                    var propertyInfos =
                        @event.Entity.GetType().GetProperties().Where(
                            info =>
                            info.CanWrite && !typeof(SystemEntity).IsAssignableFrom(info.PropertyType) &&
                            !info.PropertyType.IsGenericType && !ignorePropertyNames.Contains(info.Name)).ToList();

                    var propertyNames = @event.Persister.PropertyNames;

                    var jObject = new JObject();

                    var anyChanges = false;
                    for (int i = 0; i < propertyNames.Length; i++)
                    {
                        string propertyName = propertyNames[i];

                        if (propertyInfos.All(info => info.Name != propertyName))
                            continue;

                        var oldValue = @event.OldState[i];
                        var newValue = @event.State[i];

                        if (oldValue != null)
                            if (!oldValue.Equals(newValue))
                                anyChanges = true;

                        if (oldValue == null && newValue != null)
                            anyChanges = true;

                        jObject.Add(propertyName, new JRaw(JsonConvert.SerializeObject(oldValue)));
                    }
                    if (anyChanges)
                    {
                        var document = GetDocument(session, @event.Entity as Document);

                        var documentVersion = new DocumentVersion
                                                  {
                                                      Document = document,
                                                      Data = JsonConvert.SerializeObject(jObject),
                                                      User = GetUser(session),
                                                      CreatedOn = CurrentRequestData.Now,
                                                      UpdatedOn = CurrentRequestData.Now,
                                                      Site = session.Get<Site>(CurrentRequestData.CurrentSite.Id)
                                                  };
                        document.Versions.Add(documentVersion);
                        using (var transaction = session.BeginTransaction())
                        {
                            //session.Save(documentVersion);
                            session.Update(document);
                            transaction.Commit();
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                CurrentRequestData.ErrorSignal.Raise(exception);
            }
        }

        private Document GetDocument(ISession session, Document document)
        {
            return
                session.QueryOver<Document>().Fetch(doc => doc.Versions).Eager.Where(
                    doc => doc.Id == document.Id).List().FirstOrDefault();
        }

        private User GetUser(ISession session)
        {
            if (CurrentRequestData.CurrentUser != null)
                return session.Load<User>(CurrentRequestData.CurrentUser.Id);
            if (CurrentRequestData.CurrentContext != null && CurrentRequestData.CurrentContext.User != null)
            {
                var currentUser =
                    session.QueryOver<User>().Where(
                        user => user.Email == CurrentRequestData.CurrentContext.User.Identity.Name).
                            SingleOrDefault();
                return currentUser;
            }
            return null;
        }
    }
}