﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Mvc;

namespace PAB.API.Controllers
{
    [Route("api/[controller]")]
    public class XsrfTokenController : Controller
    {
        private readonly IAntiforgery _antiforgery;
        public XsrfTokenController(IAntiforgery antiforgery)
        {
            _antiforgery = antiforgery;
        }

        [HttpGet]
        public IActionResult Get()
        {
            var tokens = _antiforgery.GetAndStoreTokens(HttpContext);

            return new ObjectResult(new
            {
                token = tokens.RequestToken,
                tokenName = tokens.HeaderName
            });
        }
    }
}
