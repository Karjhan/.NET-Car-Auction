using System.ComponentModel.DataAnnotations;

namespace AuctionService.Entities;

public class BaseEntity
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();
}