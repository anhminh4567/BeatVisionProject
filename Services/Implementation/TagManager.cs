using AutoMapper;
using Repository.Interface;
using Shared.Helper;
using Shared.Models;
using Shared.RequestDto;
using Shared.ResponseDto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Implementation
{
	public class TagManager
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly IMapper _mapper;

		public TagManager(IUnitOfWork unitOfWork, IMapper mapper)
		{
			_unitOfWork = unitOfWork;
			_mapper = mapper;
		}

		public async Task<TagDto> Create(CreateTagDto createTagDto)
		{
			var result =  await _unitOfWork.Repositories.tagRepository.Create(new Tag() 
			{ 
				Name = createTagDto.Name
			});
			await _unitOfWork.SaveChangesAsync();
			var mappedResult = _mapper.Map<TagDto>(result);	
			return mappedResult;
		} 
		public async Task<TagDto> Remove(int tagId)
		{
			var getTag = await _unitOfWork.Repositories.tagRepository.GetById(tagId);
			if(getTag == null) {
				return null;
			}
			var result= await _unitOfWork.Repositories.tagRepository.Delete(getTag);
			await _unitOfWork.SaveChangesAsync();
			var mappedResult = _mapper.Map<TagDto>(result);
			return mappedResult;
		}
		public async Task<IList<TagDto>> GetAll()
		{
			return _mapper.Map<IList<TagDto>>( ( await _unitOfWork.Repositories.tagRepository.GetAll()).ToList() );
		}
	}
}
