using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using CoreFlogger;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using PAB.API.Model;
using PAB.Entity;
using PAB.Model;
using PAB.RepositoryInterface;
using UnprocessableEntityObjectResult = PAB.API.Helpers.UnprocessableEntityObjectResult;
using PAB.Helper;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace PAB.API.Controllers
{
    
    [Route("api/[controller]")]
    [Authorize]
    public class ContactController : Controller
    {
        private IContactNameRepo _contactNameRepo;
        private IUrlHelper _urlHelper;
        public ContactController(IContactNameRepo contactNameRepo, IUrlHelper urlHelper)
        {
            _contactNameRepo = contactNameRepo;
            _urlHelper = urlHelper;
        }

        [HttpGet("{contactId}", Name = "GetContact")]
        [TrackUsage("Persol Personal Address Book", "Core API", "Get Contact By Contact Id")]
        public async Task<IActionResult> GetCodeById(Guid contactId, [FromHeader(Name = "Accept")] string mediaType)
        {
            if (await _contactNameRepo.CodeExists(contactId) == false)
            {
                return NotFound();
            }

            //get logged in user id from claims
            var userId = new Guid(User.Claims.FirstOrDefault(c => c.Type == "sub")?.Value);

            var contactNameFromRepo = await _contactNameRepo.GetContactById(contactId, userId);

            var contactNames = Mapper.Map<ContactDto>(contactNameFromRepo);

            //return Ok(contactNames);

            return Ok(mediaType == "application/vnd.persol.hateoas+json" ? CreateLinksForContact(contactNames) : contactNames);
            //Log.Information("This is a handler for {Path}", Request.Path);
        }

        [HttpGet(Name = "GetUserContact")]
        [TrackUsage("Persol Personal Address Book", "Core API", "Get Contact User")]
        public async Task<IActionResult> GetContactByUserId()
        {
            //throw new Exception("Random exception");

            //get logged in user id from claims
            var userId = new Guid(User.Claims.FirstOrDefault(c => c.Type == "sub")?.Value);

            if (await _contactNameRepo.NameExistsAsync(userId) == false)
            {
                return NotFound();
            }

            var contactNameFromRepo = await _contactNameRepo.GetContactByUserId(userId);

            var contactNames = Mapper.Map<IList<ContactDto>>(contactNameFromRepo);

            return Ok(contactNames);

            //return Ok(mediaType == "application/vnd.persol.hateoas+json" ? CreateLinksForContact(contactNames) : contactNames);
            //Log.Information("This is a handler for {Path}", Request.Path);
        }

        [HttpGet("{userId}", Name = "SearchAllContact")]
        [TrackUsage("Persol Personal Address Book", "Core API", "Search Contact")]
        public async Task<IActionResult> GetAllContact(Guid userId, [FromQuery] ResourceParameters resourceParameters,
            [FromHeader(Name = "Accept")] string mediaType)
        {
            userId = new Guid(User.Claims.FirstOrDefault(c => c.Type == "sub")?.Value);

            if (await _contactNameRepo.NameExistsAsync(userId) == false)
            {
                return NotFound();
            }

            var contactFromRepo = await _contactNameRepo.SearchContactsAsync(userId, resourceParameters);

            var contact = Mapper.Map<IEnumerable<GetAllContactDto>>(contactFromRepo);

            if (mediaType == "application/vnd.persol.hateoas+json")
            {
                var links = CreateLinksForContact(resourceParameters, contactFromRepo.HasNext, contactFromRepo.HasPrevious);

                var paginationMetadata = new
                {
                    totalCount = contactFromRepo.TotalCount,
                    pageSize = contactFromRepo.PageSize,
                    currentPage = contactFromRepo.CurrentPage,
                    totalPages = contactFromRepo.TotalPages
                };

                Response.Headers.Add("X-Pagination", Newtonsoft.Json.JsonConvert.SerializeObject(paginationMetadata));


                contact = contact.Select(contactAll =>
                {
                    contactAll = CreateLinksForContactAll(contactAll);
                    return contactAll;
                });

                var linkedCollectionResource = new
                {
                    value = contact,
                    links
                };

                return Ok(linkedCollectionResource);
            }
            else
            {
                var previousPageLink = contactFromRepo.HasPrevious ? CreateContactResourceUri(resourceParameters, ResourceUriType.PreviousPage) : null;

                var nextPageLink = contactFromRepo.HasNext ? CreateContactResourceUri(resourceParameters, ResourceUriType.NextPage) : null;

                var paginationMetadata = new
                {
                    previousPageLink,
                    nextPageLink,
                    totalCount = contactFromRepo.TotalCount,
                    pageSize = contactFromRepo.PageSize,
                    currentPage = contactFromRepo.CurrentPage,
                    totalPages = contactFromRepo.TotalPages
                };

                Response.Headers.Add("X-Pagination",
                    Newtonsoft.Json.JsonConvert.SerializeObject(paginationMetadata));

                return Ok(contact);
            }

        }

        //[HttpGet("{id}", Name = "GetUserContactError")]
        //[TrackUsage("Persol Personal Address Book", "Core API", "Get Contact User Error")]
        //public async Task<IActionResult> GetContactErrorByUserId(Guid id)
        //{
        //    //throw new Exception("Random exception");

        //    //get logged in user id from claims
        //    var userId = new Guid(User.Claims.FirstOrDefault(c => c.Type == "sub")?.Value);

        //    if (await _contactNameRepo.NameExistsAsync(userId) == false)
        //    {
        //        return NotFound();
        //    }

        //    var contactNameFromRepo = await _contactNameRepo.GetContactByUserIdError(userId);

        //    var contactNames = Mapper.Map<IList<ContactDto>>(contactNameFromRepo);

        //    return Ok(contactNames);
        //}

        [HttpPost(Name = "CreateContact")]
        //[ValidateAntiForgeryToken]
        [TrackUsage("Persol Personal Address Book", "Core API", "Create Contact")]
        public async Task<IActionResult> CreateContact([FromBody] ContactForCreationDto contact, [FromHeader(Name = "Accept")] string mediaType)
        {
            if (contact == null)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                return new UnprocessableEntityObjectResult(ModelState);
            }

            // Check if the company id and either contactNameToReturn or description already exist
            if (await _contactNameRepo.CheckContactExistsAsync(contact.FirstName, contact.LastName, contact.UserId))
            {
                return new StatusCodeResult(StatusCodes.Status409Conflict);
            }

            var contactEntity = Mapper.Map<psPARContactName>(contact);

            //get logged in user id from claims
            var userId = User.Claims.FirstOrDefault(c => c.Type == "sub")?.Value;

            //update entity with user id
            contactEntity.IUserId = new Guid(userId);

            await _contactNameRepo.AddNewContactObject(contactEntity);

            if (await _contactNameRepo.Save() == false)
            {
                throw new Exception("Creating an contact failed on save.");
            }

            var contactNameToReturn = Mapper.Map<ContactDto>(contactEntity);

            return CreatedAtRoute("GetContact", new { contactId = contactNameToReturn.Id, contactNameToReturn.UserId },
                mediaType == "application/vnd.persol.hateoas+json" ?
                    CreateLinksForContact(contactNameToReturn) : contactNameToReturn);
        }

        [HttpPost("{contactId}", Name = " BlockContactCreation")]
        //[ValidateAntiForgeryToken]
        [TrackUsage("Persol Personal Address Book", "Core API", "Block Contact Creation")]
        public async Task<IActionResult> BlockCodesCreation(Guid contactId)
        {
            if (await _contactNameRepo.CodeExists(contactId))
            {
                return new StatusCodeResult(StatusCodes.Status409Conflict);
            }

            return NotFound();
        }

        [HttpPut("{contactId}", Name = "UpdateContact")]
        //[ValidateAntiForgeryToken]
        [TrackUsage("Persol Personal Address Book", "Core API", "Update Contact")]
        public async Task<IActionResult> UpdateContact(Guid contactId, [FromBody] ContactForUpdateDto contact)
        {
            if (contact == null)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                return new UnprocessableEntityObjectResult(ModelState);
            }

            //var userId = contact.UserId;

            //get logged in user id from claims
            contact.UserId = new Guid(User.Claims.FirstOrDefault(c => c.Type == "sub")?.Value);

            var contactFromRepo = await _contactNameRepo.GetContactById(contactId, contact.UserId);
            
            if (contactFromRepo == null)
            {   
                return NotFound();
            }

             Mapper.Map(contact, contactFromRepo);

            _contactNameRepo.Update(contactFromRepo);

            if (await _contactNameRepo.Save() == false)
            {
                throw new Exception($"Updating contact with Id: {contactId} failed on save.");
            }

            return NoContent();
        }

        [HttpPatch("{contactId}", Name = "PartiallyUpdateContact")]
        //[ValidateAntiForgeryToken]
        [TrackUsage("Persol Personal Address Book", "Core API", "Partially Updated Contact")]
        public async Task<IActionResult> PartiallyUpdatehcmcodes(Guid contactId, [FromBody] JsonPatchDocument<ContactForUpdateDto> patchDoc)
        {
            if (patchDoc == null)
            {
                return BadRequest();
            }

            //get logged in user id from claims
            var userId = new Guid(User.Claims.FirstOrDefault(c => c.Type == "sub")?.Value);

            var contactFromRepo = await _contactNameRepo.GetContactById(contactId, userId);
            if (contactFromRepo == null)
            {
                return NotFound();
            }   

            var contactToPatch = Mapper.Map<ContactForUpdateDto>(contactFromRepo);

            contactToPatch.UserId = userId;

            patchDoc.ApplyTo(contactToPatch, ModelState);

            if (!ModelState.IsValid)
            {
                return new UnprocessableEntityObjectResult(ModelState);
            }

            Mapper.Map(contactToPatch, contactFromRepo);

            if (await _contactNameRepo.Save() == false)
            {
                throw new Exception($"Patching contact with Id: {contactId} failed on save.");
            }

            return NoContent();
        }

        [HttpDelete("{userId}/{contactId}", Name = "DeleteContact")]
        //[ValidateAntiForgeryToken]
        [TrackUsage("Persol Personal Address Book", "Core API", "Deleted Contact")]
        public async Task<IActionResult> DeleteContactCreation(Guid userId, Guid contactId)
        {
            //get logged in user id from claims
            userId = new Guid(User.Claims.FirstOrDefault(c => c.Type == "sub")?.Value);

            var contactFromRepo = await _contactNameRepo.GetContactById(contactId, userId);
            if (contactFromRepo == null)
            {
                return NotFound();
            }

            _contactNameRepo.DeleteContact(contactFromRepo);

            if (await _contactNameRepo.Save() == false)
            {
                throw new Exception($"Deleting contact {contactId} failed on save.");
            }

            return NoContent();
        }

        private List<LinkDto> CreateLinksForContact(ResourceParameters contactResourceParameters, bool hasNext, bool hasPrevious)
        {
            var links = new List<LinkDto>
            {
                new LinkDto(CreateContactResourceUri(contactResourceParameters,
                        ResourceUriType.Current)
                    , "self", "GET")
            };

            // self 

            if (hasNext)
            {
                links.Add(
                    new LinkDto(CreateContactResourceUri(contactResourceParameters,
                            ResourceUriType.NextPage),
                        "nextPage", "GET"));
            }

            if (hasPrevious)
            {
                links.Add(
                    new LinkDto(CreateContactResourceUri(contactResourceParameters,
                            ResourceUriType.PreviousPage),
                        "previousPage", "GET"));
            }

            return links;
        }

        private string CreateContactResourceUri(ResourceParameters contactResourceParameters, ResourceUriType type)
        {
            switch (type)
            {
                case ResourceUriType.PreviousPage:
                    return _urlHelper.Link("SearchAllContact",
                        new
                        {
                            searchQuery = contactResourceParameters.SearchQuery,
                            pageNumber = contactResourceParameters.PageNumber - 1,
                            pageSize = contactResourceParameters.PageSize
                        });
                case ResourceUriType.NextPage:
                    return _urlHelper.Link("SearchAllContact",
                        new
                        {
                            searchQuery = contactResourceParameters.SearchQuery,
                            pageNumber = contactResourceParameters.PageNumber + 1,
                            pageSize = contactResourceParameters.PageSize
                        });
                default:
                    return _urlHelper.Link("SearchAllContact",
                        new
                        {
                            searchQuery = contactResourceParameters.SearchQuery,
                            pageNumber = contactResourceParameters.PageNumber,
                            pageSize = contactResourceParameters.PageSize
                        });
            }
        }

        private GetAllContactDto CreateLinksForContactAll(GetAllContactDto contactNameToReturn)
        {
            contactNameToReturn.Links.Add(new LinkDto(_urlHelper.Link("GetContact", new { contactId = contactNameToReturn.Id, contactNameToReturn.UserId }), "self", "GET"));

            contactNameToReturn.Links.Add(new LinkDto(_urlHelper.Link("GetUserContact", new { userId = contactNameToReturn.UserId }), "get_usercontact", "GET"));

            contactNameToReturn.Links.Add(new LinkDto(_urlHelper.Link("DeleteContact", new { codeId = contactNameToReturn.Id, contactNameToReturn.UserId }), "delete_contact", "DELETE"));

            contactNameToReturn.Links.Add(new LinkDto(_urlHelper.Link("UpdateContact", new { codeId = contactNameToReturn.Id }), "update_contact", "PUT"));

            contactNameToReturn.Links.Add(new LinkDto(_urlHelper.Link("PartiallyUpdateContact", new { codeId = contactNameToReturn.Id }), "partially_update_contact", "PATCH"));

            return contactNameToReturn;
        }

        private ContactDto CreateLinksForContact(ContactDto contactNameToReturn)
        {
            contactNameToReturn.Links.Add(new LinkDto(_urlHelper.Link("GetContact", new { contactId = contactNameToReturn.Id, contactNameToReturn.UserId }), "self", "GET"));

            contactNameToReturn.Links.Add(new LinkDto(_urlHelper.Link("GetUserContact", new { userId = contactNameToReturn.UserId }), "get_usercontact", "GET"));

            contactNameToReturn.Links.Add(new LinkDto(_urlHelper.Link("DeleteContact", new { contactId = contactNameToReturn.Id, contactNameToReturn.UserId }), "delete_contact", "DELETE"));

            contactNameToReturn.Links.Add(new LinkDto(_urlHelper.Link("UpdateContact", new { contactId = contactNameToReturn.Id }), "update_contact", "PUT"));

            contactNameToReturn.Links.Add(new LinkDto(_urlHelper.Link("PartiallyUpdateContact", new { contactId = contactNameToReturn.Id }), "partially_update_contact", "PATCH"));

            return contactNameToReturn;
        }
    }
}
