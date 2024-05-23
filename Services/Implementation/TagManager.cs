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

		public TagManager(IUnitOfWork unitOfWork)
		{
			_unitOfWork = unitOfWork;
		}
		public async Task<Tag> Create(CreateTagDto createTagDto)
		{
			var result =  await _unitOfWork.Repositories.tagRepository.Create(new Tag() 
			{ 
				Name = createTagDto.Name
			});
			await _unitOfWork.SaveChangesAsync();
			return result;
		} 
		public async Task<Tag> Remove(int tagId)
		{
			var getTag = await _unitOfWork.Repositories.tagRepository.GetById(tagId);
			if(getTag == null) {
				return null;
			}
			var result= await _unitOfWork.Repositories.tagRepository.Delete(getTag);
			await _unitOfWork.SaveChangesAsync();
			return result;
		}
		public async Task<IList<Tag>> GetAll()
		{
			return (await _unitOfWork.Repositories.tagRepository.GetAll()).ToList();
		}
	}
}
