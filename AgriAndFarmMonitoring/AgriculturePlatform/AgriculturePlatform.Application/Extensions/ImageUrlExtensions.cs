// Application/Extensions/ImageUrlExtensions.cs
using AgriculturePlatform.Application.DTOs.Harvest;
using AgriculturePlatform.Application.DTOs.Observation;
using AgriculturePlatform.Application.DTOs.Field;
using AgriculturePlatform.Application.Interfaces;
using AgriculturePlatform.Application.Common;

namespace AgriculturePlatform.Application.Extensions;

public static class ImageUrlExtensions
{
    /// <summary>
    /// Transforms image paths to public URLs for Harvest DTOs
    /// </summary>
    public static HarvestDto WithPublicUrls(this HarvestDto dto, IFileStorageService fileStorageService)
    {
        if (dto == null) return null!;

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

    /// <summary>
    /// Transforms image paths to public URLs for Observation DTOs
    /// </summary>
    public static ObservationDto WithPublicUrls(this ObservationDto dto, IFileStorageService fileStorageService)
    {
        if (dto == null) return null!;

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

    /// <summary>
    /// Transforms image paths to public URLs for a collection of Harvest DTOs
    /// </summary>
    public static IEnumerable<HarvestDto> WithPublicUrls(this IEnumerable<HarvestDto> dtos, IFileStorageService fileStorageService)
    {
        return dtos.Select(dto => dto.WithPublicUrls(fileStorageService));
    }

    /// <summary>
    /// Transforms image paths to public URLs for a collection of Observation DTOs
    /// </summary>
    public static IEnumerable<ObservationDto> WithPublicUrls(this IEnumerable<ObservationDto> dtos, IFileStorageService fileStorageService)
    {
        return dtos.Select(dto => dto.WithPublicUrls(fileStorageService));
    }

    /// <summary>
    /// Transforms image paths to public URLs for a paged result of Harvest DTOs
    /// </summary>
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

    /// <summary>
    /// Transforms image paths to public URLs for Field DTOs
    /// </summary>
    public static FieldDto WithPublicUrls(this FieldDto dto, IFileStorageService fileStorageService)
    {
        if (dto == null) return null!;

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

    /// <summary>
    /// Transforms image paths to public URLs for a collection of Field DTOs
    /// </summary>
    public static IEnumerable<FieldDto> WithPublicUrls(this IEnumerable<FieldDto> dtos, IFileStorageService fileStorageService)
    {
        return dtos.Select(dto => dto.WithPublicUrls(fileStorageService));
    }

    /// <summary>
    /// Transforms image paths to public URLs for a paged result of Field DTOs
    /// </summary>
    public static PagedResult<FieldDto> WithPublicUrls(this PagedResult<FieldDto> pagedResult, IFileStorageService fileStorageService)
    {
        if (pagedResult?.Items?.Any() == true)
        {
            pagedResult.Items = pagedResult.Items
                .Select(dto => dto.WithPublicUrls(fileStorageService))
                .ToList();
        }
        return pagedResult!;
    }

    /// <summary>
    /// Transforms image paths to public URLs for a paged result of Observation DTOs
    /// </summary>
    public static PagedResult<ObservationDto> WithPublicUrls(this PagedResult<ObservationDto> pagedResult, IFileStorageService fileStorageService)
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