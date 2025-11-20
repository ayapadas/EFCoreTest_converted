using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using EFCoreTest.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EFCoreTest.Services;

public class CodingTestService(AppDbContext db, ILogger<CodingTestService> logger) : ICodingTestService
{
    private readonly AppDbContext _db = db;
    private readonly ILogger<CodingTestService> _logger = logger;

    public async Task GeneratePostSummaryReportAsync(int maxItems)
    {
        try
        {
            var query = _db.Posts
            .AsNoTracking()
            .Select(post => new
            {
                post.Id,
                AuthorName = post.Author.Name,
                CommentCount = post.Comments.Count(),
                LatestCommentAuthor = post.Comments
                    .OrderByDescending(com => com.CreatedAt)
                    .Select(com => com.Author.Name)
                    .FirstOrDefault()
            })
            .OrderBy(p => p.Id)
            .Take(maxItems);

            Console.WriteLine("<--REPORT_START-->");

            await foreach (var p in query.AsAsyncEnumerable())
            {
                Console.WriteLine(
                    $"POST_SUMMARY|{p.Id}|{p.AuthorName}|{p.CommentCount}|{p.LatestCommentAuthor}"
                );
            }

            Console.WriteLine("<--REPORT_END-->");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating post summary report");
            throw;
        }
        // Task placeholder:
        // - Emit REPORT_START, then up to `maxItems` lines prefixed with "POST_SUMMARY|" and
        //   finally REPORT_END. Each summary line must include PostId|AuthorName|CommentCount|LatestCommentAuthor.
        // - Method must be read-only and efficient for large datasets;
        // Implement the method body in the assessment; do not change the signature.
        //throw new NotImplementedException("Implement GeneratePostSummaryReportAsync according to assessment requirements.");
    }

    public async Task<IList<PostDto>> SearchPostSummariesAsync(string query, int maxResults = 50)
    {
        try
        {
            query = query?.Trim();

            var posts = _db.Posts
            .AsNoTracking()
            .Where(p =>
                string.IsNullOrWhiteSpace(query) ||
                EF.Functions.Like(p.Title, $"%{query}%") ||
                EF.Functions.Like(p.Content, $"%{query}%")
            )
            .OrderByDescending(p => p.CreatedAt)
            .Select(p => new PostDto
            {
                Id = p.Id,
                Title = p.Title,
                Content = p.Content,
                CreatedAt = p.CreatedAt,
                AuthorName = p.Author.Name
            })
            .Take(maxResults);

            return await posts.ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching post summarys");
            throw;
        }
        // Task placeholder:
        // - Return at most `maxResults` PostDto entries.
        // - Treat null/empty/whitespace query as no filter (return unfiltered results up to maxResults).
        // - Matching: case-insensitive substring in Title OR Content.
        // - Order by CreatedAt descending, project to PostDto, and avoid materializing full entities.
        // Implement the method body in the assessment; do not change the signature.
        //throw new NotImplementedException("Implement SearchPostSummariesAsync according to assessment requirements.");
    }

    public async Task<IList<PostDto>> SearchPostSummariesAsync<TKey>(string query, int skip, int take, Expression<Func<PostDto, TKey>> orderBySelector, bool descending)
    {
        try
        {
            query = query?.Trim();

            var baseQuery = _db.Posts
                .AsNoTracking()
                .Where(p =>
                    string.IsNullOrWhiteSpace(query) ||
                    EF.Functions.Like(p.Title, $"%{query}%") ||
                    EF.Functions.Like(p.Content, $"%{query}%")
                )
                .Select(p => new PostDto
                {
                    Id = p.Id,
                    Title = p.Title,
                    Content = p.Content,
                    CreatedAt = p.CreatedAt,
                    AuthorName = p.Author.Name
                });


            IQueryable<PostDto> ordered = descending
                ? baseQuery.OrderByDescending(orderBySelector)
                : baseQuery.OrderBy(orderBySelector);

            return await ordered
                .Skip(skip)
                .Take(take)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching post summarys");
            throw;
        }
        // Task placeholder:
        // - Server-side filter by `query` (null/empty => no filter), server-side ordering based on
        //   the provided DTO selector, then Skip/Take for paging. Project to PostDto and avoid
        //   per-row queries or client-side paging.
        // - Implementations may choose which selectors to support; unsupported selectors may
        //   be rejected by the grader.
        // Implement the method body in the assessment; do not change the signature.
        //throw new NotImplementedException("Implement SearchPostSummariesAsync (paged) according to assessment requirements.");
    }
}
