using Api.Application.Services;
using Api.Application.Services.FakeDataGenerators;
using Api.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace Api.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CardsController : ControllerBase
    {
        private readonly FakeCardGeneratorService _cardService;

        public CardsController(FakeCardGeneratorService cardService)
        {
            _cardService = cardService;
        }

        [HttpGet]
        public IActionResult GetCards()
        {
            var cards = _cardService.GetCards();
            return Ok(cards);
        }
    }
}
