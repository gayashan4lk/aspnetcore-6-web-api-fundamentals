﻿using AutoMapper;
using CityInfo.API.Models;
using CityInfo.API.Services;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace CityInfo.API.Controllers
{
    [ApiController]
    [Route("api/cities")]
    public class CitiesController : ControllerBase
    {
        private readonly ILogger<CitiesController> logger;
        private readonly ICityInfoRepository cityInfoRepository;
        private readonly IMapper mapper;
        const int maxCitiesPageSize = 10;

        public CitiesController(ILogger<CitiesController> logger, ICityInfoRepository cityInfoRepository, IMapper mapper)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.cityInfoRepository = cityInfoRepository ?? throw new ArgumentNullException(nameof(cityInfoRepository));
            this.mapper = mapper;
        }

        [HttpGet()]
        public async Task<ActionResult<IEnumerable<CityWithoutPointOfInterestDto>>> GetCities(string? name, string?searchQuery, int pageSize = 10, int pageNumber = 1)
        {
            try
            {
                if (pageSize > maxCitiesPageSize)
                    pageSize = maxCitiesPageSize;
                
                var (cityEntities, paginationMetadata) = await cityInfoRepository.GetCitiesAsync(name, searchQuery, pageSize, pageNumber);

                Response.Headers.Add("X-Pagination", JsonSerializer.Serialize(paginationMetadata));

                return Ok(mapper.Map<IEnumerable<CityWithoutPointOfInterestDto>>(cityEntities));
            }
            catch (Exception ex)
            {
                logger.LogCritical("Exception occured while getting cities.", ex);
                return StatusCode(500, "A server error occured while handling your request.");
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetCity(int id, bool isPointsOfInterestIncluded = false)
        {
            try
            {
                var cityEntity = await cityInfoRepository.GetCityAsync(id, isPointsOfInterestIncluded);

                if (cityEntity == null) return NotFound();

                if (isPointsOfInterestIncluded) return Ok(mapper.Map<CityDto>(cityEntity));

                return Ok(mapper.Map<CityWithoutPointOfInterestDto>(cityEntity));
            }
            catch (Exception ex)
            {
                logger.LogCritical("Exception occured while getting a city.", ex);
                return StatusCode(500, "A server error occured while handling your request.");
            }
        }
    } 
}
