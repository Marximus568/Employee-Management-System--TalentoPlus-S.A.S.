using AutoMapper;
using Application.DTOs.Auth;
using Application.DTOs.Department;
using Application.DTOs.Employee;
using Domain.Entities;

namespace Application.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Create
        CreateMap<CreateEmployeeDto, Employee>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => "Active"));

        // Update
        CreateMap<UpdateEmployeeDto, Employee>();

        // To DTO (full detail)
        CreateMap<Employee, EmployeeDto>()
            .ForMember(dest => dest.DepartmentName,
                opt => opt.MapFrom(src => src.Department.Name));

        // Department
        CreateMap<Department, DepartmentDto>();

        // Summary DTO (for pagination)
        CreateMap<Employee, EmployeeSummaryDto>()
            .ForMember(dest => dest.FullName, 
                opt => opt.MapFrom(src => $"{src.FirstName} {src.LastName}"))
            .ForMember(dest => dest.DepartmentName,
                opt => opt.MapFrom(src => src.Department.Name));

        // Delete DTO
        CreateMap<Employee, DeleteEmployeeDto>()
            .ForMember(dest => dest.FullName,
                opt => opt.MapFrom(src => $"{src.FirstName} {src.LastName}"));
    }
}
