using EmployeeManagementSystem.DTOs;
using EmployeeManagementSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeManagementSystem.Controllers
{
    [Authorize(Roles = "Admin")]
    [ApiController]
    [Route("api/[controller]")]
    public class ProjectsController : ControllerBase
    {
        private readonly IProjectService _projectService;

        public ProjectsController(IProjectService projectService)
        {
            _projectService = projectService;
        }

        // GET ALL PROJECTS
        [HttpGet]
        public async Task<IActionResult> GetProjects()
        {
            var projects = await _projectService.GetAllProjectsAsync();
            return Ok(projects);
        }

        // GET PROJECT BY ID
        [HttpGet("{id}")]
        public async Task<IActionResult> GetProject(int id)
        {
            var project = await _projectService.GetProjectByIdAsync(id);
            return Ok(project);
        }

        // CREATE PROJECT
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> CreateProject(CreateProjectDto dto)
        {
            var project = await _projectService.CreateProjectAsync(dto);
            return Ok(project);
        }

        // UPDATE PROJECT
        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProject(int id, UpdateProjectDto dto)
        {
            var project = await _projectService.UpdateProjectAsync(id, dto);
            return Ok(project);
        }

        // DELETE PROJECT
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProject(int id)
        {
            await _projectService.DeleteProjectAsync(id);
            return Ok(new { Message = "Project deleted successfully" });
        }
    }
}

