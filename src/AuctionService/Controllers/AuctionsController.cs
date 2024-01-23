using AuctionService.Data;
using AuctionService.Data.DTOs;
using AuctionService.Entities;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Contracts;
using MassTransit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AuctionService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuctionsController(AuctionDbContext context, IMapper mapper, IPublishEndpoint publishEndpoint) : ControllerBase
    {
        private readonly AuctionDbContext _context = context;
        private readonly IMapper _mapper = mapper;
        private readonly IPublishEndpoint _publishEndpoint = publishEndpoint;

        [HttpGet]
        public async Task<ActionResult<IEnumerable<AuctionDto>>> GetAllAuctions(string date)
        {
            var query = _context.Auctions.OrderBy(x => x.Item.Make).AsQueryable();
            if (!string.IsNullOrEmpty(date))
            {
                query = query.Where(x => x.UpdatedAt.CompareTo(DateTime.Parse(date).ToUniversalTime()) > 0);
            }
            #region OldCode
            // var auctions = await _context.Auctions
            // .Include(x => x.Item)
            // .OrderBy(x => x.Item.Make)
            // .ToListAsync();
            // if (auctions.Any())
            // {
            //     var auctionDtos = _mapper.Map<IEnumerable<AuctionDto>>(auctions);
            //     return Ok(auctionDtos);
            // }
            #endregion
            return await query.ProjectTo<AuctionDto>(_mapper.ConfigurationProvider).ToListAsync();//in order to above commented code
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
        [Authorize]
        [HttpPost]
        public async Task<ActionResult<AuctionDto>> CreateAuction(CreateAuctionDto createAuctionDto)
        {
            var auction = _mapper.Map<Auction>(createAuctionDto);
            auction.Seller = User.Identity.Name;
            _context.Auctions.Add(auction);
            var newAuctionDto = _mapper.Map<AuctionDto>(auction);
            //below lines act like a transaction (because of Atomicity feature ...)
            //even service bus was stopped, below line runs and sends message to outbox and AuctionService not going to stop 
            //and message going to save in outboxMessage table and waits until service bus get ready
            await _publishEndpoint.Publish(_mapper.Map<AuctionCreated>(newAuctionDto));
            var result = await _context.SaveChangesAsync() > 0;//SaveChange method returns an integer that indicate number of items that saved to DB

            if (!result)
            {
                return BadRequest("Could not Save Changes to the DB!");
            }
            //In ASP.NET Core, the CreatedAtAction method is an action result that generates a 201 Created response,
            //indicating that the request has been successful and a new resource has been created.
            //It also includes a Location header that points to the URI of the newly created resource.
            //This allows the client to retrieve the newly created resource using a GET request.
            return CreatedAtAction(nameof(GetAuctionById), new { auction.Id }, newAuctionDto);
        }
        [Authorize]
        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateAuction(Guid id, UpdateAuctionDto updateAuctionDto)
        {
            var currentAuction = await _context.Auctions
            .Include(x => x.Item)
            .FirstOrDefaultAsync(x => x.Id == id);
            if (currentAuction == null)
            {
                return NotFound("No Item Found to Update!");
            }
            if (currentAuction.Seller != User.Identity.Name) return Forbid();
            currentAuction.Item.Model = updateAuctionDto.Model ?? currentAuction.Item.Model;
            currentAuction.Item.Make = updateAuctionDto.Make ?? currentAuction.Item.Make;
            currentAuction.Item.Color = updateAuctionDto.Color ?? currentAuction.Item.Color;
            currentAuction.Item.Mileage = updateAuctionDto.Mileage ?? currentAuction.Item.Mileage;
            currentAuction.Item.Year = updateAuctionDto.Year ?? currentAuction.Item.Year;

            await _publishEndpoint.Publish(_mapper
            .Map<AuctionUpdated>(_mapper.Map<AuctionDto>(currentAuction)));

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
            if (auction.Seller != User.Identity.Name) return Forbid();
            await _publishEndpoint.Publish(new AuctionDeleted { Id = id.ToString() });

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