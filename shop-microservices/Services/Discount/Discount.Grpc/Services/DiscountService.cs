using Discount.Grpc.Data;
using Discount.Grpc.Models;
using Grpc.Core;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace Discount.Grpc.Services;

public class DiscountService : DiscountProtoService.DiscountProtoServiceBase
{
    private readonly DiscountContext _dbContext;
    private readonly ILogger<DiscountService> _logger;

    public DiscountService(DiscountContext dbContext, ILogger<DiscountService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public override async Task<CouponModel> GetDiscount(GetDiscountRequest request, ServerCallContext context)
    {
        var coupon = await _dbContext.Coupons.FirstOrDefaultAsync(c => c.ProductName == request.ProductName);
        
        if (coupon == null)
            coupon = new Coupon { ProductName = "No Discount", Amount = 0, Description = "No Discount Description" };
        
        _logger.LogInformation("Discount is retrieved for ProductName : {productName}, Amount : {amount}", coupon.ProductName, coupon.Amount);
        
        var couponModel = coupon.Adapt<CouponModel>();
        return couponModel;
    }

    public override async Task<CouponModel> CreateDiscount(CreateDiscountRequest request, ServerCallContext context)
    {
        var coupon = request.Coupon.Adapt<Coupon>();
        if (coupon == null)
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid request object."));
        
        _dbContext.Coupons.Add(coupon);
        await _dbContext.SaveChangesAsync();
        
        _logger.LogInformation("Discount is successfully created. ProductName : {productName}", coupon.ProductName);
        
        var couponModel = coupon.Adapt<CouponModel>();
        return couponModel;
    }

    public override async Task<CouponModel> UpdateDiscount(UpdateDiscountRequest request, ServerCallContext context)
    {
        var coupon = request.Coupon.Adapt<Coupon>();
        if (coupon == null)
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid request object."));
        
        _dbContext.Coupons.Update(coupon);
        await _dbContext.SaveChangesAsync();
        
        _logger.LogInformation("Discount is successfully updated. ProductName : {productName}", coupon.ProductName);
        
        var couponModel = coupon.Adapt<CouponModel>();
        return couponModel;
    }

    public override async Task<DeleteDiscountResponse> DeleteDiscount(DeleteDiscountRequest request, ServerCallContext context)
    {
        var coupon = await _dbContext.Coupons.FirstOrDefaultAsync(c => c.ProductName == request.ProductName);
        if (coupon == null)
            throw new RpcException(new Status(StatusCode.NotFound, $"Discount with ProductName : {request.ProductName} does not exist."));
        
        _dbContext.Coupons.Remove(coupon);
        await _dbContext.SaveChangesAsync();
        
        _logger.LogInformation("Discount is successfully deleted. ProductName : {productName}", coupon.ProductName);
        
        return new DeleteDiscountResponse { Success = true };
    }
}