using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using WildNatureExplorer.Application.DTOs.Admin;
using WildNatureExplorer.Application.Interfaces.Repositories;
using WildNatureExplorer.Application.Interfaces.Services;
using WildNatureExplorer.Application.Common;
using WildNatureExplorer.Domain.Entities;
using System.Text.Json;

namespace WildNatureExplorer.Application.Services;

public class AdminImportService : IAdminImportService
{
    private readonly ISpeciesRepository _speciesRepository;
    private readonly ICountryRepository _countryRepository;
    private readonly IColorRepository _colorRepository;
    private readonly IHabitatRepository _habitatRepository;
    private readonly ISizeRepository _sizeRepository;
    private readonly ISpeciesLocationRepository _speciesLocationRepository;

    public AdminImportService(
        ISpeciesRepository speciesRepository,
        ICountryRepository countryRepository,
        IColorRepository colorRepository,
        IHabitatRepository habitatRepository,
        ISizeRepository sizeRepository,
        ISpeciesLocationRepository speciesLocationRepository)
    {
        _speciesRepository = speciesRepository;
        _countryRepository = countryRepository;
        _colorRepository = colorRepository;
        _habitatRepository = habitatRepository;
        _sizeRepository = sizeRepository;
        _speciesLocationRepository = speciesLocationRepository;
    }

    public async Task ImportSingleSpeciesAsync(AdminSpeciesImportDto dto)
    {
        var existing = await _speciesRepository.SearchNamesAsync(dto.CommonName);
        if (existing.Contains(dto.CommonName))
            throw new Common.ValidationException($"Species '{dto.CommonName}' already exists.", "SPECIES_ALREADY_EXISTS");

        var species = await MapDtoToSpeciesAsync(dto);
        await _speciesRepository.AddAsync(species);
    }

    public async Task ImportSpeciesCsvAsync(AdminSpeciesCsvDto dto)
    {
        using var reader = new StreamReader(dto.FileStream);
        using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true,
            IgnoreBlankLines = true,
            TrimOptions = TrimOptions.Trim
        });

        var records = csv.GetRecords<AdminSpeciesImportDto>().ToList();

        foreach (var record in records)
        {
            var existing = await _speciesRepository.SearchNamesAsync(record.CommonName);
            if (existing.Contains(record.CommonName))
                continue;

            var species = await MapDtoToSpeciesAsync(record);
            await _speciesRepository.AddAsync(species);
        }
    }

    public async Task ImportSpeciesLocationsCsvAsync(Guid speciesId, Stream fileStream)
    {
        using var reader = new StreamReader(fileStream);
        using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true,
            IgnoreBlankLines = true,
            TrimOptions = TrimOptions.Trim
        });

        var records = csv.GetRecords<SpeciesLocationCsvRecord>().ToList();

        foreach (var record in records)
        {
            if (record.Latitude < -90 || record.Latitude > 90)
                throw new Common.ValidationException($"Invalid Latitude: {record.Latitude}. Valid range is -90 to 90.", "INVALID_LATITUDE");
            if (record.Longitude < -180 || record.Longitude > 180)
                throw new Common.ValidationException($"Invalid Longitude: {record.Longitude}. Valid range is -180 to 180.", "INVALID_LONGITUDE");

            var location = new SpeciesLocation
            {
                SpeciesId = speciesId,
                Latitude = record.Latitude,
                Longitude = record.Longitude,
                Description = record.Description
            };

            await _speciesLocationRepository.AddAsync(location);
        }
    }

    public async Task ImportSpeciesLocationsFromGbifAsync(Guid speciesId, Guid countryId)
    {
        var species = await _speciesRepository.GetByIdAsync(speciesId);
        if (species == null)
            throw new ResourceNotFoundException("Species", speciesId.ToString());

        var country = await _countryRepository.GetByIdAsync(countryId);
        if (country == null)
            throw new ResourceNotFoundException("Country", countryId.ToString());

        var scientificName = species.ScientificName;
        var countryName = country.Name;

        var url = $"https://api.gbif.org/v1/occurrence/search?scientificName={Uri.EscapeDataString(scientificName)}&hasCoordinate=true&limit=50";

        using var http = new HttpClient();
        var response = await http.GetStringAsync(url);

        using var jsonDoc = JsonDocument.Parse(response);
        if (!jsonDoc.RootElement.TryGetProperty("results", out var results))
            return;

        foreach (var item in results.EnumerateArray())
        {
            if (!item.TryGetProperty("country", out var countryElem))
                continue;

            if (!string.Equals(countryElem.GetString(), countryName, StringComparison.OrdinalIgnoreCase))
                continue;

            if (!item.TryGetProperty("decimalLatitude", out var latElem) ||
                !item.TryGetProperty("decimalLongitude", out var lonElem))
                continue;

            var latitude = latElem.GetDouble();
            var longitude = lonElem.GetDouble();

            if (latitude < -90 || latitude > 90 || longitude < -180 || longitude > 180)
                continue;

            string description = "";

            if (item.TryGetProperty("media", out var mediaArr) && mediaArr.ValueKind == JsonValueKind.Array)
            {
                var firstMedia = mediaArr.EnumerateArray().FirstOrDefault();
                if (firstMedia.ValueKind == JsonValueKind.Object && firstMedia.TryGetProperty("identifier", out var mediaUrl))
                {
                    description += $"Photo: {mediaUrl.GetString()}; ";
                }
            }

            if (item.TryGetProperty("eventDate", out var eventDateElem) && !string.IsNullOrWhiteSpace(eventDateElem.GetString()))
            {
                description += $"Date: {eventDateElem.GetString()}; ";
            }

            if (item.TryGetProperty("recordedBy", out var recordedByElem) && !string.IsNullOrWhiteSpace(recordedByElem.GetString()))
            {
                description += $"RecordedBy: {recordedByElem.GetString()}; ";
            }
            if (item.TryGetProperty("identifiedBy", out var identifiedByElem) && !string.IsNullOrWhiteSpace(identifiedByElem.GetString()))
            {
                description += $"IdentifiedBy: {identifiedByElem.GetString()}; ";
            }

            var location = new SpeciesLocation
            {
                SpeciesId = speciesId,
                Latitude = latitude,
                Longitude = longitude,
                Description = description.Trim()
            };

            await _speciesLocationRepository.AddAsync(location);
        }
    }


    private async Task<Species> MapDtoToSpeciesAsync(AdminSpeciesImportDto dto)
    {
        var normalizedSize = dto.Size.ToUpperInvariant();
        var size = await _sizeRepository.GetByNormalizedNameAsync(normalizedSize);

        if (size == null)
        {
            size = new Size
            {
                Name = dto.Size,
                NormalizedName = normalizedSize
            };
            await _sizeRepository.AddAsync(size);
        }

        var species = new Species
        {
            CommonName = dto.CommonName,
            ScientificName = dto.ScientificName,
            Description = dto.Description ?? string.Empty,
            IsDangerous = dto.IsDangerous,
            IsRare = dto.IsRare,
            SizeId = size.Id
        };

        foreach (var c in SplitValues(dto.Countries ?? string.Empty))
        {
            var normalized = c.ToUpperInvariant();

            var country = await _countryRepository.GetByNormalizedNameAsync(normalized)
                ?? await CreateCountryAsync(c, normalized);

            species.Countries.Add(new SpeciesCountry
            {
                SpeciesId = species.Id,
                CountryId = country.Id
            });
        }

        foreach (var c in SplitValues(dto.Colors ?? string.Empty))
        {
            var normalized = c.ToUpperInvariant();
            var color = await _colorRepository.GetByNormalizedNameAsync(normalized)
                ?? await CreateColorAsync(c, normalized);

            species.Colors.Add(new SpeciesColor
            {
                SpeciesId = species.Id,
                ColorId = color.Id
            });
        }

        foreach (var h in SplitValues(dto.Habitats ?? string.Empty))
        {
            var normalized = h.ToUpperInvariant();
            var habitat = await _habitatRepository.GetByNormalizedNameAsync(normalized)
                ?? await CreateHabitatAsync(h, normalized);

            species.Habitats.Add(new SpeciesHabitat
            {
                SpeciesId = species.Id,
                HabitatId = habitat.Id
            });
        }

        return species;
    }

    private async Task<Country> CreateCountryAsync(string name, string normalized)
    {
        var country = new Country { Name = name, NormalizedName = normalized };
        await _countryRepository.AddAsync(country);
        return country;
    }

    private async Task<Color> CreateColorAsync(string name, string normalized)
    {
        var color = new Color { Name = name, NormalizedName = normalized };
        await _colorRepository.AddAsync(color);
        return color;
    }

    private async Task<Habitat> CreateHabitatAsync(string name, string normalized)
    {
        var habitat = new Habitat { Name = name, NormalizedName = normalized };
        await _habitatRepository.AddAsync(habitat);
        return habitat;
    }

    private static IEnumerable<string> SplitValues(string raw)
    {
        return raw
            .Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(x => x.Trim())
            .Where(x => !string.IsNullOrWhiteSpace(x));
    }
}
