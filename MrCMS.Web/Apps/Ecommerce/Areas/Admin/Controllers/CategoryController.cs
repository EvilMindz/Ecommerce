﻿using System.Collections.Generic;
using System.Web.Mvc;
using MrCMS.Entities.Documents.Web;
using MrCMS.Services;
using MrCMS.Web.Apps.Ecommerce.Pages;
using MrCMS.Web.Apps.Ecommerce.Services;
using MrCMS.Website.Controllers;

namespace MrCMS.Web.Apps.Ecommerce.Areas.Admin.Controllers
{
    public class CategoryController : MrCMSAppAdminController<EcommerceApp>
    {
        private readonly ICategoryService _categoryService;
        private readonly IDocumentService _documentService;

        public CategoryController(ICategoryService categoryService, IDocumentService documentService)
        {
            _categoryService = categoryService;
            _documentService = documentService;
        }

        public ViewResult Index(string q = null, int p = 1)
        {
            if (_documentService.GetUniquePage<CategoryContainer>() == null)
                return View();
            var categoryPagedList = _categoryService.Search(q, p);
            return View(categoryPagedList);
        }

        public JsonResult Search(string term, List<int> ids)
        {
            return Json(_categoryService.Search(term, ids ?? new List<int>()));
        }
    }
}