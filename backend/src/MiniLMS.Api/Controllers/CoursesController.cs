using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using MiniLMS.Core.DTOs;
using MiniLMS.Core.Interfaces;
using MiniLMS.Core.Models;

namespace MiniLMS.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CoursesController : ControllerBase
{
    private readonly ICourseRepository _courseRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<CoursesController> _logger;

    public CoursesController(ICourseRepository courseRepository, IMapper mapper, ILogger<CoursesController> logger)
    {
        _courseRepository = courseRepository;
        _mapper = mapper;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<CourseDto>>> GetCourses()
    {
        try
        {
            var courses = await _courseRepository.GetAllAsync();
            var courseDtos = _mapper.Map<IEnumerable<CourseDto>>(courses);
            return Ok(courseDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving courses");
            return StatusCode(500, "An error occurred while retrieving courses");
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<CourseDto>> GetCourse(int id)
    {
        try
        {
            var course = await _courseRepository.GetByIdAsync(id);
            if (course == null)
            {
                return NotFound();
            }

            var courseDto = _mapper.Map<CourseDto>(course);
            return Ok(courseDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error retrieving course {id}");
            return StatusCode(500, "An error occurred while retrieving the course");
        }
    }

    [HttpPost]
    public async Task<ActionResult<CourseDto>> CreateCourse(CreateCourseDto createCourseDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var course = _mapper.Map<Course>(createCourseDto);
            var createdCourse = await _courseRepository.CreateAsync(course);
            var courseDto = _mapper.Map<CourseDto>(createdCourse);

            return CreatedAtAction(nameof(GetCourse), new { id = courseDto.Id }, courseDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating course");
            return StatusCode(500, "An error occurred while creating the course");
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<CourseDto>> UpdateCourse(int id, UpdateCourseDto updateCourseDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var existingCourse = await _courseRepository.GetByIdAsync(id);
            if (existingCourse == null)
            {
                return NotFound();
            }

            _mapper.Map(updateCourseDto, existingCourse);
            existingCourse.Id = id; // Ensure ID is preserved

            var updatedCourse = await _courseRepository.UpdateAsync(existingCourse);
            var courseDto = _mapper.Map<CourseDto>(updatedCourse);

            return Ok(courseDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error updating course {id}");
            return StatusCode(500, "An error occurred while updating the course");
        }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteCourse(int id)
    {
        try
        {
            var course = await _courseRepository.GetByIdAsync(id);
            if (course == null)
            {
                return NotFound();
            }

            await _courseRepository.DeleteAsync(id);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error deleting course {id}");
            return StatusCode(500, "An error occurred while deleting the course");
        }
    }
}
