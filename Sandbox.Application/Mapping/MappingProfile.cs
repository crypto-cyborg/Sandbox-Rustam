using AutoMapper;
using Sandbox.Core.Entities;
using Sandbox.Core.Enums;
using Sandbox.Shared.DTOs;

namespace Sandbox.Application.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Order, OrderDto>()
                .ForMember(dest => dest.OrderType, opt => opt.MapFrom(src => src.Type.ToString()))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
                .ForMember(dest => dest.Direction, opt => opt.MapFrom(src => src.Direction.ToString()))
                .ReverseMap()
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => Enum.Parse<OrderType>(src.OrderType)))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => Enum.Parse<OrderStatus>(src.Status)))
                .ForMember(dest => dest.Direction, opt => opt.MapFrom(src => Enum.Parse<PositionDirection>(src.Direction)));
            CreateMap<ClosedOrder, OrderDto>()
                .ForMember(dest => dest.OrderType, opt => opt.MapFrom(src => src.Type.ToString()))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
                .ForMember(dest => dest.Direction, opt => opt.MapFrom(src => src.Direction.ToString()))
                .ReverseMap()
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => Enum.Parse<OrderType>(src.OrderType)))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => Enum.Parse<OrderStatus>(src.Status)))
                .ForMember(dest => dest.Direction, opt => opt.MapFrom(src => Enum.Parse<PositionDirection>(src.Direction)));

            CreateMap<Position, PositionDto>()
                .ForMember(dest => dest.AverageEntryPrice, opt => opt.MapFrom(src => src.AverageEntryPrice))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
                .ForMember(dest => dest.Direction, opt => opt.MapFrom(src => src.Direction.ToString()))
                .ForMember(dest => dest.StopLossPrice, opt => opt.MapFrom(src => src.StopLossOrder != null ? src.StopLossOrder.Price : (decimal?)null))
                .ForMember(dest => dest.TakeProfitPrice, opt => opt.MapFrom(src => src.TakeProfitOrder != null ? src.TakeProfitOrder.Price : (decimal?)null))
                .ForMember(dest => dest.Pnl, opt => opt.MapFrom(src => src.CalculatePnL()))
                .ForMember(dest => dest.PnlPercentage, opt => opt.MapFrom(src => src.CalculatePnLPercentage()))
                .ReverseMap()
                .ForMember(dest => dest.AverageEntryPrice, opt => opt.MapFrom(src => src.AverageEntryPrice))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => Enum.Parse<PositionStatus>(src.Status)))
                .ForMember(dest => dest.Direction, opt => opt.MapFrom(src => Enum.Parse<PositionDirection>(src.Direction)));
            CreateMap<ClosedPosition, PositionDto>()
                .ForMember(dest => dest.AverageEntryPrice, opt => opt.MapFrom(src => src.AverageEntryPrice))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
                .ForMember(dest => dest.Direction, opt => opt.MapFrom(src => src.Direction.ToString()))
                .ForMember(dest => dest.Pnl, opt => opt.MapFrom(src => src.CalculatePnL()))
                .ForMember(dest => dest.PnlPercentage, opt => opt.MapFrom(src => src.CalculatePnLPercentage()))
                .ReverseMap()
                .ForMember(dest => dest.AverageEntryPrice, opt => opt.MapFrom(src => src.AverageEntryPrice))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => Enum.Parse<PositionStatus>(src.Status)))
                .ForMember(dest => dest.Direction, opt => opt.MapFrom(src => Enum.Parse<PositionDirection>(src.Direction)));

            CreateMap<Account, AccountDto>()
                .ForMember(dest => dest.WalletId, opt => opt.MapFrom(src => src.Wallet.Id))
                .ReverseMap();
            
            CreateMap<CreateAccountDto, Account>()
                .ForMember(dest => dest.Wallet, opt => opt.MapFrom(src => new Wallet()));

            CreateMap<Wallet, WalletDto>().ReverseMap();
        }
    }
}
