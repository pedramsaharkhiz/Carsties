using AuctionService.Data;
using AuctionService.Data.DTOs;
using AuctionService.Entities;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AuctionService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuctionsController(AuctionDbContext context, IMapper mapper) : ControllerBase
    {
        private readonly AuctionDbContext _context = context;
        private readonly IMapper _mapper = mapper;
        [HttpGet]
        public async Task<ActionResult<IEnumerable<AuctionDto>>> GetAllAuctions()
        {
            var auctions = await _context.Auctions
            .Include(x => x.Item)
            .OrderBy(x => x.Item.Make)
            .ToListAsync();
            if (auctions.Any())
            {
                var auctionDtos = _mapper.Map<IEnumerable<AuctionDto>>(auctions);
                return Ok(auctionDtos);
            }
            return NotFound();
        }
        [HttpGet("{id}")]
        public async Task<ActionResult<AuctionDto>> GetAuctionById(Guid id)
        {
            var auction = await _context.Auctions
            .Include(x => x.Item)
            .FirstOrDefaultAsync(x => x.Id == id);
            if (auction != null)
            {
                var auctionDto = _mapper.Map<AuctionDto>(auction);
                return Ok(auctionDto);
            }
            return NotFound();
        }
        [HttpPost]
        public async Task<ActionResult<AuctionDto>> CreateAuction(CreateAuctionDto createAuctionDto)
        {
            var auction = _mapper.Map<Auction>(createAuctionDto);
            auction.Seller = "test";
            _context.Auctions.Add(auction);
            var result = await _context.SaveChangesAsync() > 0;//SaveChange method returns an integer that indicate number of items that saved to DB
            if (!result)
            {
                return BadRequest("Could not Save Changes to the DB!");
            }
            //In ASP.NET Core, the CreatedAtAction method is an action result that generates a 201 Created response,
            //indicating that the request has been successful and a new resource has been created.
            //It also includes a Location header that points to the URI of the newly created resource.
            //This allows the client to retrieve the newly created resource using a GET request.
            return CreatedAtAction(nameof(GetAuctionById), new { auction.Id }, _mapper.Map<AuctionDto>(auction));
        }
        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateAuction(Guid id, UpdateAuctionDto updateAuctionDto)
        {
            var currentAuction = await _context.Auctions
            .Include(x => x.Item)
            .FirstOrDefaultAsync(x => x.Id == id);
            if (currentAuction == null)
            {
                return BadRequest("No Item Found to Update!");
            }
            currentAuction.Item.Model = updateAuctionDto.Model ?? currentAuction.Item.Model;
            currentAuction.Item.Make = updateAuctionDto.Make ?? currentAuction.Item.Make;
            currentAuction.Item.Color = updateAuctionDto.Color ?? currentAuction.Item.Color;
            currentAuction.Item.Mileage = updateAuctionDto.Mileage ?? currentAuction.Item.Mileage;
            currentAuction.Item.Year = updateAuctionDto.Year ?? currentAuction.Item.Year;

            var result = await _context.SaveChangesAsync() > 0;
            if (!result)
            {
                return BadRequest("Could not Update!");
            }
            return Ok();

        }
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteAuction(Guid id)
        {
            var auction = _context.Auctions
            .Include(x => x.Item)
            .FirstOrDefault(x => x.Id == id);
            if (auction == null)
            {
                return NotFound("Item Not Found!");
            }
            _context.Remove(auction);
            var result = await _context.SaveChangesAsync() > 0;
            if (!result)
            {
                return BadRequest("Could not Delete Item!");
            }
            return Ok();
        }

    }
}