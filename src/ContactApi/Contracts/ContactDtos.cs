namespace ContactApi.Contracts;

public record CreateContactRequest(string Name, string PhoneNumber, string? Email);
public record UpdateContactRequest(string Name, string PhoneNumber, string? Email);
public record ContactResponse(Guid Id, string Name, string PhoneNumber, string? Email, DateTimeOffset CreatedAt, DateTimeOffset? UpdatedAt);
