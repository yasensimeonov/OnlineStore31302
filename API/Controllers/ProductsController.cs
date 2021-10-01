using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.Dtos;
using API.Errors;
using API.Helpers;
using AutoMapper;
using Core.Entities;
using Core.Interfaces;
using Core.Specifications;
using Infrastructure.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    public class ProductsController : BaseApiController
    {
        // private readonly IProductRepository _repo;
        
        // private readonly IGenericRepository<Product> _productsRepo;
        // private readonly IGenericRepository<ProductBrand> _productBrandRepo;
        // private readonly IGenericRepository<ProductType> _productTypeRepo;
        
        private readonly IMapper _mapper;

        // public ProductsController(IGenericRepository<Product> productsRepo, IGenericRepository<ProductBrand> productBrandRepo, 
        //                           IGenericRepository<ProductType> productTypeRepo, IMapper mapper)
        // {
        //     _mapper = mapper;
        //     _productTypeRepo = productTypeRepo;
        //     _productBrandRepo = productBrandRepo;
        //     _productsRepo = productsRepo;
        //     // _repo = repo;
        // }
        private readonly IUnitOfWork _unitOfWork;

        public ProductsController(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        // [Cached(600)]
        [HttpGet]
        public async Task<ActionResult<Pagination<ProductToReturnDto>>> GetProducts([FromQuery] ProductSpecParams productParams)
        {
            var spec = new ProductsWithTypesAndBrandsSpecification(productParams);

            var countSpec = new ProductWithFiltersForCountSpecification(productParams);

            // var totalItems = await _productsRepo.CountAsync(countSpec);
            var totalItems = await _unitOfWork.Repository<Product>().CountAsync(countSpec);

            // var products = await _repo.GetProductsAsync();
            // var products = await _productsRepo.ListAllAsync();

            // var products = await _productsRepo.ListAsync(spec);
            var products = await _unitOfWork.Repository<Product>().ListAsync(spec);

            var data = _mapper.Map<IReadOnlyList<Product>, IReadOnlyList<ProductToReturnDto>>(products);

            // return Ok(products);

            // return products.Select(product => new ProductToReturnDto
            // {
            //     Id = product.Id,
            //     Name = product.Name,
            //     Description = product.Description,
            //     PictureUrl = product.PictureUrl,
            //     Price = product.Price,
            //     ProductBrand = product.ProductBrand.Name,
            //     ProductType = product.ProductType.Name
            // }).ToList();

            return Ok(new Pagination<ProductToReturnDto>(productParams.PageIndex, productParams.PageSize, totalItems, data));
        }

        [Cached(600)]
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ProductToReturnDto>> GetProduct(int id)
        {
            var spec = new ProductsWithTypesAndBrandsSpecification(id);

            // return await _repo.GetProductByIdAsync(id);
            // return await _productsRepo.GetByIdAsync(id);
            // return await _productsRepo.GetEntityWithSpec(spec);

            // var product = await _productsRepo.GetEntityWithSpec(spec);
            var product = await _unitOfWork.Repository<Product>().GetEntityWithSpec(spec);

            // return new ProductToReturnDto
            // {
            //     Id = product.Id,
            //     Name = product.Name,
            //     Description = product.Description,
            //     PictureUrl = product.PictureUrl,
            //     Price = product.Price,
            //     ProductBrand = product.ProductBrand.Name,
            //     ProductType = product.ProductType.Name
            // };

            if (product == null)
            {
                return NotFound(new ApiResponse(404));
            }

            return _mapper.Map<Product, ProductToReturnDto>(product);
        }

        [Cached(600)]
        [HttpGet("brands")]
        public async Task<ActionResult<IReadOnlyList<ProductBrand>>> GetProductBrands()
        {
            // return Ok(await _repo.GetProductBrandsAsync());
            
            // return Ok(await _productBrandRepo.ListAllAsync());
            return Ok(await _unitOfWork.Repository<ProductBrand>().ListAllAsync());
        }

        [Cached(600)]
        [HttpGet("types")]
        public async Task<ActionResult<IReadOnlyList<ProductType>>> GetProductTypes()
        {
            // return Ok(await _repo.GetProductTypesAsync());
            
            // return Ok(await _productTypeRepo.ListAllAsync());
            return Ok(await _unitOfWork.Repository<ProductType>().ListAllAsync());
        }

        [HttpPost]
        public async Task<ActionResult<Product>> CreateProduct(ProductCreateDto productToCreate)
        {
            var product = _mapper.Map<ProductCreateDto, Product>(productToCreate);
            product.PictureUrl = "images/products/placeholder.png";

            _unitOfWork.Repository<Product>().Add(product);

            var result = await _unitOfWork.Complete();

            if (result <= 0)
            {
                return BadRequest(new ApiResponse(400, "Problem creating product"));
            }

            return Ok(product);
        }
        
        [HttpPut("{id}")]
        public async Task<ActionResult<Product>> UpdateProduct(int id, ProductCreateDto productToUpdate)
        {
            var product = await _unitOfWork.Repository<Product>().GetByIdAsync(id);

            if (productToUpdate.PictureUrl == null)
            {
                productToUpdate.PictureUrl = "images/products/placeholder.png";
            }

            _mapper.Map(productToUpdate, product);

            _unitOfWork.Repository<Product>().Update(product);

            var result = await _unitOfWork.Complete();

            if (result <= 0)
            {
                return BadRequest(new ApiResponse(400, "Problem updating product"));
            } 

            return Ok(product);
        }
        
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteProduct(int id)
        {
            var product = await _unitOfWork.Repository<Product>().GetByIdAsync(id);
            
            _unitOfWork.Repository<Product>().Delete(product);

            var result = await _unitOfWork.Complete();
            
            if (result <= 0)
            {
                return BadRequest(new ApiResponse(400, "Problem deleting product"));
            }

            return Ok();
        }

    }
}