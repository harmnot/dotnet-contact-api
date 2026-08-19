using ContactApi.Contracts;
using ContactApi.Domain;
using ContactApi.Data;
using Microsoft.EntityFrameworkCore;

namespace ContactApi.Endpoints;

public static class ContactEndpoints
{
    public static void MapContactEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/contacts").WithTags("Contacts");

        group.MapGet("/", async (AppDbContext db, CancellationToken ct) =>
            TypedResults.Ok(await db.Contacts.AsNoTracking()
                .Select(c => ToResponse(c))
                .ToListAsync(ct)));

        group.MapGet("/{id:guid}", async Task<IResult> (Guid id, AppDbContext db, CancellationToken ct) =>
        {
            var contact = await db.Contacts.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id, ct);
            return contact is not null ? TypedResults.Ok(ToResponse(contact)) : TypedResults.Problem(statusCode: 404, detail: "Contact not found");
        });

        group.MapPost("/", async Task<IResult> (CreateContactRequest req, AppDbContext db, CancellationToken ct) =>
        {
            if (string.IsNullOrWhiteSpace(req.Name) || string.IsNullOrWhiteSpace(req.PhoneNumber))
            {
                return TypedResults.ValidationProblem(new Dictionary<string, string[]>
                { ["name"] = ["name or phone number is required"] });
            }

            var contact = new Contact
            {
                Id = Guid.CreateVersion7(),
                Name = req.Name.Trim(),
                PhoneNumber = req.PhoneNumber.Trim(),
                Email = req.Email?.Trim(),
                CreatedAt = DateTimeOffset.UtcNow
            };

            db.Contacts.Add(contact);
            await db.SaveChangesAsync(ct);

            return TypedResults.Created($"/api/v1/contacts/{contact.Id}", ToResponse(contact));
        });

        group.MapPut("/{id:guid}", async Task<IResult> (Guid id, UpdateContactRequest req, AppDbContext db, CancellationToken ct) =>
        {
            var contact = await db.Contacts.FirstOrDefaultAsync(c => c.Id == id, ct);
            if (contact is null)
            {
                return TypedResults.Problem(statusCode: 404, detail: "Contact not found");
            }

            contact.Name = req.Name.Trim();
            contact.PhoneNumber = req.PhoneNumber.Trim();
            contact.Email = req.Email?.Trim();
            contact.UpdatedAt = DateTimeOffset.UtcNow;

            await db.SaveChangesAsync(ct);

            return TypedResults.Ok(ToResponse(contact));
        });

        group.MapDelete("/{id:guid}", async Task<IResult> (Guid id, AppDbContext db, CancellationToken ct) =>
        {
            var contact = await db.Contacts.Where(c => c.Id == id).ExecuteDeleteAsync(ct);
            return contact > 0 ? TypedResults.NoContent() : TypedResults.Problem(statusCode: 404, detail: "Contact not found");
        });
    }

    private static ContactResponse ToResponse(Contact contact) => new(contact.Id, contact.Name, contact.PhoneNumber, contact.Email, contact.CreatedAt, contact.UpdatedAt);
}
