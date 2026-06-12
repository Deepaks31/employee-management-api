using EmployeeManagementSystem.DTOs;
using EmployeeManagementSystem.Models;
using EmployeeManagementSystem.Repositories;

namespace EmployeeManagementSystem.Services
{
    public interface IProjectService
    {
        Task<IEnumerable<Project>> GetAllProjectsAsync();
        Task<Project> GetProjectByIdAsync(int id);
        Task<Project> CreateProjectAsync(CreateProjectDto dto);
        Task<Project> UpdateProjectAsync(int id, UpdateProjectDto dto);
        Task DeleteProjectAsync(int id);
    }

    public class ProjectService : IProjectService
    {
        private readonly IUnitOfWork _unitOfWork;

        public ProjectService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<Project>> GetAllProjectsAsync()
        {
            return await _unitOfWork.Projects.GetAllAsync();
        }

        public async Task<Project> GetProjectByIdAsync(int id)
        {
            var project = await _unitOfWork.Projects.GetByIdAsync(id);
            if (project == null) throw new Exception("Project not found");
            return project;
        }

        public async Task<Project> CreateProjectAsync(CreateProjectDto dto)
        {
            var project = new Project { ProjectName = dto.ProjectName };
            await _unitOfWork.Projects.AddAsync(project);
            await _unitOfWork.CompleteAsync();
            return project;
        }

        public async Task<Project> UpdateProjectAsync(int id, UpdateProjectDto dto)
        {
            var project = await _unitOfWork.Projects.GetByIdAsync(id);
            if (project == null) throw new Exception("Project not found");
            
            project.ProjectName = dto.ProjectName;
            _unitOfWork.Projects.Update(project);
            await _unitOfWork.CompleteAsync();
            
            return project;
        }

        public async Task DeleteProjectAsync(int id)
        {
            var project = await _unitOfWork.Projects.GetByIdAsync(id);
            if (project == null) throw new Exception("Project not found");

            _unitOfWork.Projects.Remove(project);
            await _unitOfWork.CompleteAsync();
        }
    }
}

