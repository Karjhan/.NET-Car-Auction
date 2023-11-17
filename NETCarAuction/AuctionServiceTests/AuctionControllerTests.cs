using AuctionService.Controllers;
using AuctionService.Data;
using AuctionService.DTO;
using AuctionService.Entities;
using AuctionService.Helpers;
using AuctionServiceTests.Utils;
using AutoFixture;
using AutoMapper;
using MassTransit;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace AuctionServiceTests;

public class AuctionControllerTests
{
    private readonly Mock<IAuctionRepository> _auctionRepo;
    private readonly Mock<IPublishEndpoint> _publishEndpoint;
    private readonly Fixture _fixture;
    private readonly AuctionsController _controller;
    private readonly IMapper _mapper;
    
    public AuctionControllerTests()
    {
        _fixture = new Fixture();
        _auctionRepo = new Mock<IAuctionRepository>();
        _publishEndpoint = new Mock<IPublishEndpoint>();
        var mockMapper = new MapperConfiguration(mc =>
        {
            mc.AddMaps(typeof(MappingProfile).Assembly);
        }).CreateMapper().ConfigurationProvider;
        _mapper = new Mapper(mockMapper);
        _controller = new AuctionsController(_auctionRepo.Object, _mapper, _publishEndpoint.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext{User = Helpers.GetClaimsPrincipal()}
            }
        };
    }

    [Fact]
    public async Task AuctionsController_GetAuctions_WithNoParams_Returns10Auctions()
    {
        //Arrange
        var auctions = _fixture.CreateMany<AuctionDTO>(10).ToList();
        _auctionRepo.Setup(repo => repo.GetAuctionsAsync(null)).ReturnsAsync(auctions);
        
        //Act
        var result = await _controller.GetAllAuctions(null);
        
        //Assert
        Assert.Equal(10, result.Value.Count);
        Assert.IsType<ActionResult<List<AuctionDTO>>>(result);
    }
    
    [Fact]
    public async Task AuctionsController_GetAuctionById_WithValidGuid_ReturnsAuctionDTO()
    {
        //Arrange
        var auction = _fixture.Create<AuctionDTO>();
        _auctionRepo.Setup(repo => repo.GetAuctionByIdAsync(It.IsAny<Guid>())).ReturnsAsync(auction);
        
        //Act
        var result = await _controller.GetAuctionById(auction.Id);
        
        //Assert
        Assert.Equal(auction.Make, result.Value.Make);
        Assert.IsType<ActionResult<AuctionDTO>>(result);
    }
    
    [Fact]
    public async Task AuctionsController_GetAuctionById_WithInvalidGuid_ReturnsNotFound()
    {
        //Arrange
        _auctionRepo.Setup(repo => repo.GetAuctionByIdAsync(It.IsAny<Guid>())).ReturnsAsync(value: null);
        
        //Act
        var result = await _controller.GetAuctionById(Guid.NewGuid());
        
        //Assert
        Assert.IsType<NotFoundResult>(result.Result);
    }
    
    [Fact]
    public async Task AuctionsController_CreateAuction_WithValidCreateAuctionDTO_ReturnsOK()
    {
        //Arrange
        var auction = _fixture.Create<CreateAuctionDTO>();
        _auctionRepo.Setup(repo => repo.AddAuction(It.IsAny<Auction>()));
        _auctionRepo.Setup(repo => repo.SaveChangesAsync()).ReturnsAsync(true);
        
        //Act
        var result = await _controller.CreateAuction(auction);
        var createdResult = result.Result as CreatedAtActionResult;
        
        //Assert
        Assert.NotNull(createdResult);
        Assert.Equal("GetAuctionById", createdResult.ActionName);
        Assert.IsType<AuctionDTO>(createdResult.Value);
    }
    
    [Fact]
    public async Task AuctionsController_CreateAuction_FailedSave_Returns400BadRequest()
    {
        //Arrange
        var auctionDTO = _fixture.Create<CreateAuctionDTO>();
        _auctionRepo.Setup(repo => repo.AddAuction(It.IsAny<Auction>()));
        _auctionRepo.Setup(repo => repo.SaveChangesAsync()).ReturnsAsync(false);
        
        //Act
        var result = _controller.CreateAuction(auctionDTO);
        
        //Assert
        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public async Task AuctionsController_UpdateAuction_WithUpdateAuctionDto_ReturnsOkResponse()
    {
        //Arrange
        var auction = _fixture.Build<Auction>().Without(x => x.Item).Create();
        auction.Item = _fixture.Create<Item>();
        auction.Seller = "test";
        var updateDto = _fixture.Create<UpdateAuctionDTO>();
        _auctionRepo.Setup(repo => repo.GetAuctionEntityById(It.IsAny<Guid>())).ReturnsAsync(auction);
        _auctionRepo.Setup(repo => repo.SaveChangesAsync()).ReturnsAsync(true);

        //Act
        var result = await _controller.UpdateAuction(auction.Id, updateDto);

        //Assert
        Assert.IsType<OkResult>(result); 
    }

    [Fact]
    public async Task AuctionsController_UpdateAuction_WithInvalidUser_Returns403Forbid()
    {
        //Arrange
        var auction = _fixture.Build<Auction>().Without(x => x.Item).Create();
        auction.Seller = "not-test";
        var updateDto = _fixture.Create<UpdateAuctionDTO>();
        _auctionRepo.Setup(repo => repo.GetAuctionEntityById(It.IsAny<Guid>())).ReturnsAsync(auction);

        //Act
        var result = await _controller.UpdateAuction(auction.Id, updateDto);

        //Assert
        Assert.IsType<ForbidResult>(result); 
    }

    [Fact]
    public async Task AuctionsController_UpdateAuction_WithInvalidGuid_ReturnsNotFound()
    {
        //Arrange
        var auction = _fixture.Build<Auction>().Without(x => x.Item).Create();
        var updateDto = _fixture.Create<UpdateAuctionDTO>();
        _auctionRepo.Setup(repo => repo.GetAuctionEntityById(It.IsAny<Guid>())).ReturnsAsync(value: null);

        //Assert
        var result = await _controller.UpdateAuction(auction.Id, updateDto);

        //Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task AuctionsController_DeleteAuction_WithValidUser_ReturnsOkResponse()
    {
        //Arrange
        var auction = _fixture.Build<Auction>().Without(x => x.Item).Create();
        auction.Seller = "test";
        _auctionRepo.Setup(repo => repo.GetAuctionEntityById(It.IsAny<Guid>())).ReturnsAsync(auction);
        _auctionRepo.Setup(repo => repo.SaveChangesAsync()).ReturnsAsync(true);

        //Act
        var result = await _controller.DeleteAuction(auction.Id);

        //Assert
        Assert.IsType<OkResult>(result);  
    }

    [Fact]
    public async Task AuctionsController_DeleteAuction_WithInvalidGuid_Returns404Response()
    {
        //Arrange
        var auction = _fixture.Build<Auction>().Without(x => x.Item).Create();
        _auctionRepo.Setup(repo => repo.GetAuctionEntityById(It.IsAny<Guid>())).ReturnsAsync(value: null);

        //Act
        var result = await _controller.DeleteAuction(auction.Id);

        //Assert
        Assert.IsType<NotFoundResult>(result); 
    }

    [Fact]
    public async Task AuctionsController_DeleteAuction_WithInvalidUser_Returns403Response()
    {
        //Arrange
        var auction = _fixture.Build<Auction>().Without(x => x.Item).Create();
        auction.Seller = "not-test";
        _auctionRepo.Setup(repo => repo.GetAuctionEntityById(It.IsAny<Guid>())).ReturnsAsync(auction);

        //Act
        var result = await _controller.DeleteAuction(auction.Id);

        //Assert
        Assert.IsType<ForbidResult>(result); 
    }
}