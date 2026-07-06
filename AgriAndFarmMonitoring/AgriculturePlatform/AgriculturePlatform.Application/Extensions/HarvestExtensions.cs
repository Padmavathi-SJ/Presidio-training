// Application/Extensions/HarvestExtensions.cs
using AgriculturePlatform.Application.DTOs.Harvest;
using AgriculturePlatform.Application.Interfaces;
using AgriculturePlatform.Application.Common;

namespace AgriculturePlatform.Application.Extensions;

public static class HarvestExtensions
{
    public static HarvestDto WithPublicUrls(this HarvestDto dto, IFileStorageService fileStorageService)
    {
        if (dto == null) return null!;

        // ✅ Only transform if the path doesn't already start with http
        if (!string.IsNullOrEmpty(dto.ImagePath) && !dto.ImagePath.StartsWith("http"))
        {
            dto.ImagePath = fileStorageService.GetPublicUrl(dto.ImagePath);
        }

        if (!string.IsNullOrEmpty(dto.ThumbnailPath) && !dto.ThumbnailPath.StartsWith("http"))
        {
            dto.ThumbnailPath = fileStorageService.GetPublicUrl(dto.ThumbnailPath);
        }

        if (dto.AdditionalImagePaths?.Any() == true)
        {
            var transformedPaths = new List<string>();
            foreach (var path in dto.AdditionalImagePaths)
            {
                // ✅ Only transform if path doesn't already start with http
                if (!string.IsNullOrEmpty(path) && !path.StartsWith("http"))
                {
                    transformedPaths.Add(fileStorageService.GetPublicUrl(path));
                }
                else
                {
                    transformedPaths.Add(path);
                }
            }
            dto.AdditionalImagePaths = transformedPaths;
        }

        return dto;
    }

    public static IEnumerable<HarvestDto> WithPublicUrls(this IEnumerable<HarvestDto> dtos, IFileStorageService fileStorageService)
    {
        return dtos.Select(dto => dto.WithPublicUrls(fileStorageService));
    }

    public static PagedResult<HarvestDto> WithPublicUrls(this PagedResult<HarvestDto> pagedResult, IFileStorageService fileStorageService)
    {
        if (pagedResult?.Items?.Any() == true)
        {
            pagedResult.Items = pagedResult.Items
                .Select(dto => dto.WithPublicUrls(fileStorageService))
                .ToList();
        }
        return pagedResult!;
    }
}