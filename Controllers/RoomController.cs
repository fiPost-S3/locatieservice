﻿using LocatieService.Database.Contexts;
using LocatieService.Database.Converters;
using LocatieService.Database.Datamodels;
using LocatieService.Database.Datamodels.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LocatieService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RoomController : Controller
    {
        private readonly LocatieContext _context;
        private readonly IDtoConverter<Room, RoomRequest, RoomResponse> _converter;

        public RoomController(LocatieContext context, IDtoConverter<Room, RoomRequest, RoomResponse> converter)
        {
            _context = context;
            _converter = converter;
        }

        [HttpPost]
        public async Task<ActionResult> CreateRoom(RoomRequest request)
        {
            Room Room = _converter.DtoToModel(request);

            Building building = await _context.Buildings.FirstOrDefaultAsync(e => e.Id == request.BuildingId);

            // Check if building exists.
            if (building == null)
            {
                return BadRequest("This building does not exist");
            }

            _context.Rooms.Add(Room);
            await _context.SaveChangesAsync();

            return Created("Created", request);
        }

        [HttpGet]
        public async Task<ActionResult<List<RoomResponse>>> GetAllRooms()
        {
            List<Room> rooms = await _context.Rooms.ToListAsync();
            List<RoomResponse> responses = new();

            foreach (Room room in rooms)
            {
                RoomResponse response = _converter.ModelToDto(room);
                response.Building = await _context.Buildings.FirstOrDefaultAsync(e => e.Id == room.BuildingId);
                responses.Add(response);
            }

            return Ok(responses);
        }

        [HttpGet]
        [Route("{id}")]
        public async Task<ActionResult<RoomResponse>> GetRoomById(Guid id)
        {
            Room room = await _context.Rooms.FirstOrDefaultAsync(e => e.Id == id);
            RoomResponse response = _converter.ModelToDto(room);
            response.Building = await _context.Buildings.FirstOrDefaultAsync(e => e.Id == room.BuildingId); // Insert building to model.

            return Ok(response);
        }

        [HttpDelete]
        [Route("{id}")]
        public async Task<ActionResult> DeleteRoomById(Guid id)
        {
            Room room = await _context.Rooms.FirstOrDefaultAsync(e => e.Id == id);

            if (room == null) // Check if room exists.
            {
                return NotFound("Object not found");
            }

            _context.Remove(room); // Remove record.
            _context.SaveChanges();

            return Ok("Successfully removed.");
        }
    }
}
