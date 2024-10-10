using Microsoft.AspNetCore.Mvc;
using Api.Application.Services;
using Api.Domain.Entities;

namespace Api.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AffiliatesController : ControllerBase
    {
        private readonly FakeAffiliateGeneratorService _affiliateService;

        public AffiliatesController(FakeAffiliateGeneratorService affiliateService)
        {
            _affiliateService = affiliateService;
        }

        [HttpGet]
        public IActionResult GetAffiliates()
        {
            var affiliates = _affiliateService.GetAffiliates();
            return Ok(affiliates); 
        }
    }
}