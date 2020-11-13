﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IdentityServer4.Services;
using Microsoft.AspNetCore.Mvc;
using PAB.Persol.IDP.Services;
using Microsoft.AspNetCore.Http;


namespace PAB.Persol.IDP.Controllers.UserRegistration
{
    public class UserRegistrationController : Controller
    {
        private readonly IPABUserRepository _pabUserRepository;
        private readonly IIdentityServerInteractionService _interaction;

        public UserRegistrationController(IPABUserRepository pabUserRepository, IIdentityServerInteractionService interaction)
        {
            _pabUserRepository = pabUserRepository;
            _interaction = interaction;
        }

        [HttpGet]
        public IActionResult RegisterUser(string returnUrl)
        {
            var vm = new RegisterUserViewModel()
            { ReturnUrl = returnUrl };
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegisterUser(RegisterUserViewModel model)
        {
            if (ModelState.IsValid)
            {
                // create user + claims
                var userToCreate = new Entities.User();
                userToCreate.Password = model.Password;
                userToCreate.Username = model.Username;
                userToCreate.IsActive = true;
                userToCreate.Claims.Add(new Entities.UserClaim("country", model.Country));
                userToCreate.Claims.Add(new Entities.UserClaim("address", model.Address));
                userToCreate.Claims.Add(new Entities.UserClaim("given_name", model.Firstname));
                userToCreate.Claims.Add(new Entities.UserClaim("family_name", model.Lastname));
                userToCreate.Claims.Add(new Entities.UserClaim("email", model.Email));
                userToCreate.Claims.Add(new Entities.UserClaim("subscriptionlevel", "FreeUser"));

                // add it through the repository
                _pabUserRepository.AddUser(userToCreate);

                if (!await _pabUserRepository.Save())
                {
                    throw new Exception($"Creating a user failed.");
                }

                // log the user in
                await HttpContext.SignInAsync(userToCreate.SubjectId, userToCreate.Username);

                //await HttpContext.SignInAsync(userToCreate.SubjectId, userToCreate.Username);

                //await HttpContext.Authentication.AuthenticateAsync

                // continue with the flow     
                if (_interaction.IsValidReturnUrl(model.ReturnUrl) || Url.IsLocalUrl(model.ReturnUrl))
                {
                    return Redirect(model.ReturnUrl);
                }

                return Redirect("~/");
            }

            // ModelState invalid, return the view with the passed-in model
            // so changes can be made
            return View(model);
        }
    }
}
