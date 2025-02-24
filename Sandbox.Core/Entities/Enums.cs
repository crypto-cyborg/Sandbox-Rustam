namespace Sandbox.Core.Entities;

public enum OrderType
{
    Market,
    Limit
}

public enum OrderStatus
{
    Open,
    Closed,
    Cancelled
}

public enum PositionStatus
{
    Open,
    Closed,
    Liquidated
}