using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Olimpo.ProductAPI.Model.DTOs;
using Olimpo.ProductAPI.Repository;

namespace Olimpo.ProductAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoriesController : ControllerBase
    {
        private readonly ICategoryRepository _repository;
        private readonly IMapper _mapper;
        public CategoriesController(ICategoryRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<CategoryDTO>>> GetAll()
        {
            var categories = await _repository.GetAllAsync();
            var categoriesDto = _mapper.Map<IEnumerable<CategoryDTO>>(categories);
            return Ok(categoriesDto);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<CategoryDTO>> GetById(long id)
        {
            var category = await _repository.GetByIdAsync(id);
            if (category == null)
            {
                return NotFound(new { message = "Categoria não encontrada" });
            }
            var categoryDto = _mapper.Map<CategoryDTO>(category);
            return Ok(categoryDto);
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        public async Task<ActionResult<CategoryDTO>> Create([FromBody] CreateCategoryDTO categoryCreateDto)
        {
            var category = _mapper.Map<Model.Entities.Category>(categoryCreateDto);
            var createdCategory = await _repository.CreateAsync(category);
            var categoryDto = _mapper.Map<CategoryDTO>(createdCategory);
            return CreatedAtAction(nameof(GetById), new { id = categoryDto.Id }, categoryDto);
        }

        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<CategoryDTO>> Update(long id, [FromBody] UpdateCategoryDTO categoryUpdateDto)
        {
            var existingCategory = await _repository.GetByIdAsync(id);
            if (existingCategory == null)
            {
                return NotFound(new { message = "Categoria não encontrada" });
            }
            _mapper.Map(categoryUpdateDto, existingCategory);
            await _repository.UpdateAsync(existingCategory);
            var categoryDto = _mapper.Map<CategoryDTO>(existingCategory);
            return Ok(categoryDto);
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(long id)
        {
            var deleted = await _repository.GetByIdAsync(id);

            if(deleted == null)
                return NotFound(new { message = "Categoria não encontrada" });

            return NoContent();
        }
    }
}
