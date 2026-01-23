using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Olimpo.ProductAPI.Model.DTOs;
using Olimpo.ProductAPI.Model.Entities;
using Olimpo.ProductAPI.Repository;

namespace Olimpo.ProductAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly IProductRepository _repository;
        private readonly IMapper _mapper;
        private readonly ICategoryRepository _categoryRepository;

        public ProductsController(IProductRepository repository, IMapper mapper, ICategoryRepository categoryRepository)
        {
            _repository = repository;
            _mapper = mapper;
            _categoryRepository = categoryRepository;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<ProductDTO>>> GetAll()
        {
            var products = await _repository.GetAllAsync();
            var productsDto = _mapper.Map<IEnumerable<ProductDTO>>(products);
            return Ok(productsDto);

        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ProductDTO>> GetById(long id)
        {
            var product = await _repository.GetByIdAsync(id);
            if (product == null)
            {
                return NotFound(new { message = "Produto não encontrado" });
            }
            var productDto = _mapper.Map<ProductDTO>(product);
            return Ok(productDto);
        }

        [HttpGet("category/{categoryId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<ProductDTO>>> GetByCategoryId(long categoryId)
        {
            var products = await _repository.GetByCategoryIdAsync(categoryId);
            var productsDto = _mapper.Map<IEnumerable<ProductDTO>>(products);
            return Ok(productsDto);
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ProductDTO>> Create([FromBody] CreateProductDTO productCreateDto)
        {
            var categoryExists = await _categoryRepository.ExistsAsync(productCreateDto.CategoryId);
            if (!categoryExists)
            {
                return BadRequest(new { message = "Categoria inválida" });
            }
            var product = _mapper.Map<Product>(productCreateDto);
            var createdProduct = await _repository.CreateAsync(product);
            var productDto = _mapper.Map<ProductDTO>(createdProduct);
            return CreatedAtAction(nameof(GetById), new { id = productDto.Id }, productDto);
        }

        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ProductDTO>> Update(long id, [FromBody] CreateProductDTO productUpdateDto)
        {
            var existingProduct = await _repository.GetByIdAsync(id);
            if (existingProduct == null)
            {
                return NoContent();
            }
            _mapper.Map(productUpdateDto, existingProduct);
            await _repository.UpdateAsync(existingProduct);
            return Ok();
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> Delete(long id)
        {
            var existingProduct = await _repository.GetByIdAsync(id);
            if (existingProduct == null)
            {
                return NotFound(new { message = "Produto não encontrado" });
            }
            await _repository.DeleteAsync(existingProduct);
            return NoContent();
        }
    }
}
