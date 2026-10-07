<?php
declare(strict_types=1);

final class SelectionPresenter
{
    public static function offerOwner(array $item): string
    {
        $name = trim((string) ($item['license_first_name'] ?? '') . ' ' . (string) ($item['license_last_name'] ?? ''));
        if ($name !== '') return $name;
        $email = trim((string) ($item['license_email'] ?? ''));
        return $email !== '' ? $email : (string) ($item['display_name'] ?? '-');
    }

    public static function get(array $data, string $key, $default = null)
    {
        foreach ($data as $name => $value) {
            if (strcasecmp((string) $name, $key) === 0) return $value;
        }
        return $default;
    }

    public static function selection(array $payload): array
    {
        $nested = self::get($payload, 'selection');
        return is_array($nested) ? $nested : $payload;
    }

    public static function snapshot(array $payload): ?array
    {
        $value = self::get($payload, 'snapshot');
        return is_array($value) ? $value : null;
    }

    public static function unit(array $payload): string
    {
        $selection = self::selection($payload);
        $unit = self::get($selection, 'unit', []);
        if (!is_array($unit)) return '-';
        return self::first([$unit], ['name', 'code', 'managementCode']) ?: '-';
    }

    public static function customerReference(array $payload): string
    {
        return (string) self::get(self::selection($payload), 'customerReference', '');
    }

    public static function scenario(array $payload, string $name): array
    {
        $value = self::get(self::selection($payload), $name, []);
        return is_array($value) ? $value : [];
    }

    public static function waterCoil(array $payload): array
    {
        $value = self::get(self::selection($payload), 'waterCoil', []);
        return is_array($value) ? $value : [];
    }

    public static function electricHeater(array $payload): array
    {
        $value = self::get(self::selection($payload), 'electricHeater', []);
        return is_array($value) ? $value : [];
    }

    public static function accessories(array $payload): array
    {
        $selection = self::selection($payload);
        $value = self::get($selection, 'accessories', []);
        $items = is_array($value) ? array_values(array_filter($value, 'is_array')) : [];

        $waterCoil = self::waterCoil($selection);
        if (filter_var(self::get($waterCoil, 'enabled'), FILTER_VALIDATE_BOOLEAN)) {
            self::appendTechnicalAccessory($items,
                (string) self::get($waterCoil, 'calculationMode', 'HCD'),
                self::technicalAccessoryName((string) self::get($waterCoil, 'calculationMode', 'HCD')),
                (string) self::get($waterCoil, 'installationType', 'Internal'));
        }

        $electricHeater = self::electricHeater($selection);
        foreach (['PEHD', 'EHD'] as $mode) {
            $modeSelection = self::get($electricHeater, $mode, []);
            if (!is_array($modeSelection) || !filter_var(self::get($modeSelection, 'enabled'), FILTER_VALIDATE_BOOLEAN)) continue;
            self::appendTechnicalAccessory($items, $mode,
                self::technicalAccessoryName($mode),
                (string) self::get($modeSelection, 'installationType', 'Internal'));
        }
        return $items;
    }

    public static function accessoryName(array $accessory): string
    {
        $name = trim((string) self::get($accessory, 'localizedDisplayName', ''));
        return $name !== '' ? $name : (string) self::get($accessory, 'code', '-');
    }

    public static function accessoryFunctions(array $accessory): array
    {
        $functions = self::get($accessory, 'localizedFunctionNames', []);
        if (!is_array($functions)) return [];
        return array_values(array_filter(array_map('strval', $functions), static function (string $value): bool {
            return trim($value) !== '';
        }));
    }

    public static function accessoryStatus(array $accessory): string
    {
        $availability = (string) self::get($accessory, 'availability', 'Optional');
        $installation = (string) self::get($accessory, 'installationType', 'External');
        if (strcasecmp($availability, 'Standard') === 0) return 'Di serie';
        if (strcasecmp($installation, 'Internal') === 0) return 'Interna';
        if (strcasecmp($installation, 'RequestedInternal') === 0) return 'Richiesta interna';
        return 'Esterna';
    }

    public static function accessoryStatusClass(array $accessory): string
    {
        if (strcasecmp((string) self::get($accessory, 'availability'), 'Standard') === 0) return 'standard';
        return strcasecmp((string) self::get($accessory, 'installationType'), 'Internal') === 0
            ? 'internal'
            : 'external';
    }

    public static function bool($value): string
    {
        return filter_var($value, FILTER_VALIDATE_BOOLEAN) ? 'Sì' : 'No';
    }

    public static function value($value, string $unit = ''): string
    {
        if ($value === null || $value === '') return '-';
        if (is_bool($value)) return self::bool($value);
        if (is_float($value)) $value = number_format($value, 2, ',', '.');
        return (string) $value . ($unit !== '' ? ' ' . $unit : '');
    }

    public static function reference(string $digits, int $revision): string
    {
        return implode('-', str_split($digits, 4)) . '-R' . str_pad((string) $revision, 2, '0', STR_PAD_LEFT);
    }

    public static function location(array $row): string
    {
        $parts = array_filter([(string) ($row['geo_country_code'] ?? ''), (string) ($row['geo_city'] ?? '')]);
        return $parts ? implode(' · ', $parts) : 'Non disponibile';
    }

    public static function comparisonRows(array $payload): array
    {
        $selection = self::selection($payload);
        $rows = ['Riferimento cliente' => self::get($selection, 'customerReference'), 'Unità' => self::unit($payload)];
        foreach (['winter' => 'Inverno', 'summer' => 'Estate'] as $key => $caption) {
            $scenario = self::scenario($payload, $key);
            $rows[$caption . ' · attivo'] = self::bool(self::get($scenario, 'enabled'));
            $rows[$caption . ' · norma'] = self::get($scenario, 'standardCode');
            $rows[$caption . ' · portata mandata'] = self::value(self::get($scenario, 'supplyAirflowM3h'), 'm³/h');
            $rows[$caption . ' · portata ripresa'] = self::value(self::get($scenario, 'extractAirflowM3h'), 'm³/h');
            $rows[$caption . ' · pressione'] = self::value(self::get($scenario, 'maximumPressurePa'), 'Pa');
            $rows[$caption . ' · aria esterna'] = self::value(self::get($scenario, 'outdoorTemperatureC'), '°C');
            $rows[$caption . ' · aria ripresa'] = self::value(self::get($scenario, 'returnTemperatureC'), '°C');
        }
        $coil = self::waterCoil($payload);
        $coilRef = self::get($coil, 'coil', []); $coilRef = is_array($coilRef) ? $coilRef : [];
        $geometry = self::get($coil, 'geometry', []); $geometry = is_array($geometry) ? $geometry : [];
        $rows['Batteria · attiva'] = self::bool(self::get($coil, 'enabled'));
        $rows['Batteria · modello'] = self::get($coilRef, 'name') ?: self::get($coilRef, 'code');
        $rows['Batteria · installazione'] = self::get($coil, 'installationType');
        $rows['Batteria · caso'] = self::get($coil, 'selectionCase');
        $rows['Batteria · modo'] = self::get($coil, 'calculationMode');
        $rows['Batteria · lunghezza'] = self::value(self::get($geometry, 'lengthMm'), 'mm');
        $rows['Batteria · altezza'] = self::value(self::get($geometry, 'heightMm'), 'mm');
        $rows['Batteria · ranghi'] = self::value(self::get($geometry, 'numberOfRows'));
        $rows['Batteria · circuiti'] = self::value(self::get($geometry, 'numberOfCircuits'));
        $rows['Accessori'] = implode(', ', array_map(static function (array $accessory): string {
            $code = (string) self::get($accessory, 'code', '-');
            $quantity = max(1, (int) self::get($accessory, 'quantity', 1));
            return $code . ($quantity > 1 ? ' x' . $quantity : '');
        }, self::accessories($payload)));
        return $rows;
    }

    private static function first(array $containers, array $keys): string
    {
        foreach ($containers as $container) foreach ($keys as $key) {
            $value = self::get($container, $key);
            if ($value !== null && trim((string) $value) !== '') return (string) $value;
        }
        return '';
    }

    private static function appendTechnicalAccessory(array &$items, string $code, string $name, string $installation): void
    {
        $code = trim($code);
        if ($code === '') return;
        foreach ($items as $item) {
            if (strcasecmp((string) self::get($item, 'code', ''), $code) === 0) return;
        }
        $items[] = [
            'Code' => $code,
            'Quantity' => 1,
            'Availability' => 'Selected',
            'InstallationType' => $installation,
            'LocalizedDisplayName' => trim($name) !== '' ? $name : $code,
            'LocalizedFunctionNames' => [],
        ];
    }

    private static function technicalAccessoryName(string $code): string
    {
        $names = [
            'CWD' => 'Batteria ad acqua per raffreddamento',
            'HWD' => 'Batteria ad acqua per riscaldamento',
            'HCD' => 'Batteria ad acqua a 2 tubi per riscaldamento e raffreddamento',
            'EHD' => 'Batteria elettrica di post-riscaldamento',
            'PEHD' => 'Batteria elettrica di preriscaldamento',
        ];
        $normalized = strtoupper(trim($code));
        return $names[$normalized] ?? $code;
    }
}
