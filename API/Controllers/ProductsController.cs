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
using Microsoft.AspNetCore.Authorization;
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
        private readonly IPhotoService _photoService;

        public ProductsController(IUnitOfWork unitOfWork, IMapper mapper, IPhotoService photoService)
        {
            _photoService = photoService;
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
        [Authorize(Roles = "Admin")]
        // public async Task<ActionResult<Product>> CreateProduct(ProductCreateDto productToCreate)
        public async Task<ActionResult<ProductToReturnDto>> CreateProduct(ProductCreateDto productToCreate)
        {
            var product = _mapper.Map<ProductCreateDto, Product>(productToCreate);
            // product.PictureUrl = "images/products/placeholder.png";

            _unitOfWork.Repository<Product>().Add(product);

            var result = await _unitOfWork.Complete();

            if (result <= 0)
            {
                return BadRequest(new ApiResponse(400, "Problem creating product"));
            }

            // return Ok(product);
            return Ok(_mapper.Map<Product, ProductToReturnDto>(product));
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        // public async Task<ActionResult<Product>> UpdateProduct(int id, ProductCreateDto productToUpdate)
        public async Task<ActionResult<ProductToReturnDto>> UpdateProduct(int id, ProductCreateDto productToUpdate)
        {
            var productBeforeUpdate = await _unitOfWork.Repository<Product>().GetByIdAsync(id);

            // if (productToUpdate.PictureUrl == null)
            // {
            //     productToUpdate.PictureUrl = "images/products/placeholder.png";
            // }
            // else
            // {
            //     productToUpdate.PictureUrl = product.PictureUrl;
            // }

            _mapper.Map(productToUpdate, productBeforeUpdate);

            _unitOfWork.Repository<Product>().Update(productBeforeUpdate);

            var result = await _unitOfWork.Complete();

            if (result <= 0)
            {
                return BadRequest(new ApiResponse(400, "Problem updating product"));
            }

            // return Ok(product);
            var spec = new ProductsWithTypesAndBrandsSpecification(id);
            var productAfterUpdate = await _unitOfWork.Repository<Product>().GetEntityWithSpec(spec);
            
            return Ok(_mapper.Map<Product, ProductToReturnDto>(productAfterUpdate));
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
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

        [HttpPut("{id}/photo")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ProductToReturnDto>> AddProductPhoto(int id, [FromForm]ProductPhotoDto photoDto)
        {
            var spec = new ProductsWithTypesAndBrandsSpecification(id);
            var product = await _unitOfWork.Repository<Product>().GetEntityWithSpec(spec);

            if (photoDto.Photo.Length > 0)
            {
                var photo = await _photoService.SaveToDiskAsync(photoDto.Photo);

                if (photo != null)
                {
                    product.AddPhoto(photo.PictureUrl, photo.FileName);

                    _unitOfWork.Repository<Product>().Update(product);
                
                    var result = await _unitOfWork.Complete();
                
                    if (result <= 0) 
                    {
                        return BadRequest(new ApiResponse(400, "Problem adding photo product"));
                    }                    
                }
                else
                {
                    return BadRequest(new ApiResponse(400, "problem saving photo to disk"));
                }
            }
            
            return _mapper.Map<Product, ProductToReturnDto>(product);
        }

        [HttpDelete("{id}/photo/{photoId}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> DeleteProductPhoto(int id, int photoId)
        {
            var spec = new ProductsWithTypesAndBrandsSpecification(id);
            var product = await _unitOfWork.Repository<Product>().GetEntityWithSpec(spec);
            
            var photo = product.Photos.SingleOrDefault(x => x.Id == photoId);

            if (photo != null)
            {
                if (photo.IsMain)
                {
                    return BadRequest(new ApiResponse(400, "You cannot delete the main photo"));
                }
                    
                _photoService.DeleteFromDisk(photo);
            }
            else
            {
                return BadRequest(new ApiResponse(400, "Photo does not exist"));
            }

            product.RemovePhoto(photoId);
            
            _unitOfWork.Repository<Product>().Update(product);
            
            var result = await _unitOfWork.Complete();
            
            if (result <= 0) 
            {
                return BadRequest(new ApiResponse(400, "Problem deleting photo for product"));
            }
            
            return Ok();
        }

        [HttpPost("{id}/photo/{photoId}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ProductToReturnDto>> SetMainPhoto(int id, int photoId)
        {
            var spec = new ProductsWithTypesAndBrandsSpecification(id);
            var product = await _unitOfWork.Repository<Product>().GetEntityWithSpec(spec);

            if (product.Photos.All(x => x.Id != photoId)) 
            {
                return NotFound();
            }
                        
            product.SetMainPhoto(photoId);
            
            _unitOfWork.Repository<Product>().Update(product);
            
            var result = await _unitOfWork.Complete();
            
            if (result <= 0)
            {
                return BadRequest(new ApiResponse(400, "Problem setting photo as main for this product"));
            }
            
            return _mapper.Map<Product, ProductToReturnDto>(product);
        }

    }
}