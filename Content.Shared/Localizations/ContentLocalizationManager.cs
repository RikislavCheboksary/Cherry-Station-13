// SPDX-FileCopyrightText: 2021 20kdc <asdd2808@gmail.com>
// SPDX-FileCopyrightText: 2021 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 E F R <602406+Efruit@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 Galactic Chimp <63882831+GalacticChimp@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2021 Vera Aguilera Puerto <6766154+Zumorica@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Kara <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2022 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 wrexbe <81056464+wrexbe@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Visne <39844191+Visne@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 moonheart08 <moonheart08@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 KrasnoshchekovPavel <119816022+KrasnoshchekovPavel@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Stalen <33173619+stalengd@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 icekot8 <93311212+icekot8@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Myra <vasilis@pikachu.systems>
// SPDX-FileCopyrightText: 2025 pathetic meowmeow <uhhadd@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using Robust.Shared.Utility;

namespace Content.Shared.Localizations
{
    public sealed class ContentLocalizationManager
    {
        [Dependency] private readonly ILocalizationManager _loc = default!;

        // If you want to change your codebase's language, do it here.
        private const string Culture = "en-US";

        /// <summary>
        /// Custom format strings used for parsing and displaying minutes:seconds timespans.
        /// </summary>
        public static readonly string[] TimeSpanMinutesFormats = new[]
        {
            @"m\:ss",
            @"mm\:ss",
            @"%m",
            @"mm"
        };

        public void Initialize()
        {
            var culture = new CultureInfo(Culture);

            _loc.LoadCulture(culture);
            _loc.AddFunction(culture, "PRESSURE", FormatPressure);
            _loc.AddFunction(culture, "POWERWATTS", FormatPowerWatts);
            _loc.AddFunction(culture, "POWERJOULES", FormatPowerJoules);
            // NOTE: ENERGYWATTHOURS() still takes a value in joules, but formats as watt-hours.
            _loc.AddFunction(culture, "ENERGYWATTHOURS", FormatEnergyWattHours);
            _loc.AddFunction(culture, "UNITS", FormatUnits);
            _loc.AddFunction(culture, "TOSTRING", args => FormatToString(culture, args));
            _loc.AddFunction(culture, "LOC", FormatLoc);
            _loc.AddFunction(culture, "NATURALFIXED", FormatNaturalFixed);
            _loc.AddFunction(culture, "NATURALPERCENT", FormatNaturalPercent);
            _loc.AddFunction(culture, "PLAYTIME", FormatPlaytime);


            /*
             * The following language functions are specific to the english localization. When working on your own
             * localization you should NOT modify these, instead add new functions specific to your language/culture.
             * This ensures the english translations continue to work as expected when fallbacks are needed.
             */
            var cultureEn = new CultureInfo("en-US");

            _loc.AddFunction(cultureEn, "MAKEPLURAL", FormatMakePlural);
            _loc.AddFunction(cultureEn, "MANY", FormatMany);
        }

        private ILocValue FormatMany(LocArgs args)
        {
            var count = ((LocValueNumber) args.Args[1]).Value;

            if (Math.Abs(count - 1) < 0.0001f)
            {
                return (LocValueString) args.Args[0];
            }
            else
            {
                return (LocValueString) FormatMakePlural(args);
            }
        }

        private ILocValue FormatNaturalPercent(LocArgs args)
        {
            var number = ((LocValueNumber) args.Args[0]).Value * 100;
            var maxDecimals = (int)Math.Floor(((LocValueNumber) args.Args[1]).Value);
            var formatter = (NumberFormatInfo)NumberFormatInfo.GetInstance(CultureInfo.GetCultureInfo(Culture)).Clone();
            formatter.NumberDecimalDigits = maxDecimals;
            return new LocValueString(string.Format(formatter, "{0:N}", number).TrimEnd('0').TrimEnd(char.Parse(formatter.NumberDecimalSeparator)) + "%");
        }

        private ILocValue FormatNaturalFixed(LocArgs args)
        {
            var number = ((LocValueNumber) args.Args[0]).Value;
            var maxDecimals = (int)Math.Floor(((LocValueNumber) args.Args[1]).Value);
            var formatter = (NumberFormatInfo)NumberFormatInfo.GetInstance(CultureInfo.GetCultureInfo(Culture)).Clone();
            formatter.NumberDecimalDigits = maxDecimals;
            return new LocValueString(string.Format(formatter, "{0:N}", number).TrimEnd('0').TrimEnd(char.Parse(formatter.NumberDecimalSeparator)));
        }

        private static readonly Regex PluralEsRule = new("^.*(s|sh|ch|x|z)$");

        private ILocValue FormatMakePlural(LocArgs args)
        {
            var text = ((LocValueString) args.Args[0]).Value;
            var split = text.Split(" ", 1);
            var firstWord = split[0];
            if (PluralEsRule.IsMatch(firstWord))
            {
                if (split.Length == 1)
                    return new LocValueString($"{firstWord}es");
                else
                    return new LocValueString($"{firstWord}es {split[1]}");
            }
            else
            {
                if (split.Length == 1)
                    return new LocValueString($"{firstWord}s");
                else
                    return new LocValueString($"{firstWord}s {split[1]}");
            }
        }

        // TODO: allow fluent to take in lists of strings so this can be a format function like it should be.
        /// <summary>
        /// Formats a list as per english grammar rules.
        /// </summary>
        public static string FormatList(List<string> list)
        {
            return list.Count switch
            {
                <= 0 => string.Empty,
                1 => list[0],
                2 => $"{list[0]} and {list[1]}",
                _ => $"{string.Join(", ", list.GetRange(0, list.Count - 1))}, and {list[^1]}"
            };
        }

        /// <summary>
        /// Formats a list as per english grammar rules, but uses or instead of and.
        /// </summary>
        public static string FormatListToOr(List<string> list)
        {
            return list.Count switch
            {
                <= 0 => string.Empty,
                1 => list[0],
                2 => $"{list[0]} or {list[1]}",
                _ => $"{string.Join(" or ", list)}"
            };
        }

        /// <summary>
        /// Formats a direction struct as a human-readable string.
        /// </summary>
        public static string FormatDirection(Direction dir)
        {
            return Loc.GetString($"zzzz-fmt-direction-{dir.ToString()}");
        }

        /// <summary>
        /// Formats playtime as hours and minutes.
        /// </summary>
        public static string FormatPlaytime(TimeSpan time)
        {
            time = TimeSpan.FromMinutes(Math.Ceiling(time.TotalMinutes));
            var hours = (int)time.TotalHours;
            var minutes = time.Minutes;
            return Loc.GetString($"zzzz-fmt-playtime", ("hours", hours), ("minutes", minutes));
        }

        private static ILocValue FormatLoc(LocArgs args)
        {
            var id = ((LocValueString) args.Args[0]).Value;

            return new LocValueString(Loc.GetString(id, args.Options.Select(x => (x.Key, x.Value.Value!)).ToArray()));
        }

        private static ILocValue FormatToString(CultureInfo culture, LocArgs args)
        {
            var arg = args.Args[0];
            var fmt = ((LocValueString) args.Args[1]).Value;

            var obj = arg.Value;
            if (obj is IFormattable formattable)
                return new LocValueString(formattable.ToString(fmt, culture));

            return new LocValueString(obj?.ToString() ?? "");
        }

        private static ILocValue FormatUnitsGeneric(
            LocArgs args,
            string mode,
            Func<double, double>? transformValue = null)
        {
            const int maxPlaces = 5; // Matches amount in _lib.ftl
            var pressure = ((LocValueNumber) args.Args[0]).Value;

            if (transformValue != null)
                pressure = transformValue(pressure);

            var places = 0;
            while (pressure > 1000 && places < maxPlaces)
            {
                pressure /= 1000;
                places += 1;
            }

            return new LocValueString(Loc.GetString(mode, ("divided", pressure), ("places", places)));
        }

        private static ILocValue FormatPressure(LocArgs args)
        {
            return FormatUnitsGeneric(args, "zzzz-fmt-pressure");
        }

        private static ILocValue FormatPowerWatts(LocArgs args)
        {
            return FormatUnitsGeneric(args, "zzzz-fmt-power-watts");
        }

        private static ILocValue FormatPowerJoules(LocArgs args)
        {
            return FormatUnitsGeneric(args, "zzzz-fmt-power-joules");
        }

        private static ILocValue FormatEnergyWattHours(LocArgs args)
        {
            const double joulesToWattHours = 1.0 / 3600;

            return FormatUnitsGeneric(args, "zzzz-fmt-energy-watt-hours", joules => joules * joulesToWattHours);
        }

        private static ILocValue FormatUnits(LocArgs args)
        {
            if (!Units.Types.TryGetValue(((LocValueString) args.Args[0]).Value, out var ut))
                throw new ArgumentException($"Unknown unit type {((LocValueString) args.Args[0]).Value}");

            var fmtstr = ((LocValueString) args.Args[1]).Value;

            double max = Double.NegativeInfinity;
            var iargs = new double[args.Args.Count - 1];
            for (var i = 2; i < args.Args.Count; i++)
            {
                var n = ((LocValueNumber) args.Args[i]).Value;
                if (n > max)
                    max = n;

                iargs[i - 2] = n;
            }

            if (!ut.TryGetUnit(max, out var mu))
                throw new ArgumentException("Unit out of range for type");

            var fargs = new object[iargs.Length];

            for (var i = 0; i < iargs.Length; i++)
                fargs[i] = iargs[i] * mu.Factor;

            fargs[^1] = Loc.GetString($"units-{mu.Unit.ToLower()}");

            // Before anyone complains about "{"+"${...}", at least it's better than MS's approach...
            // https://docs.microsoft.com/en-us/dotnet/standard/base-types/composite-formatting#escaping-braces
            //
            // Note that the closing brace isn't replaced so that format specifiers can be applied.
            var res = String.Format(
                fmtstr.Replace("{UNIT", "{" + $"{fargs.Length - 1}"),
                fargs
            );

            return new LocValueString(res);
        }

        private static ILocValue FormatPlaytime(LocArgs args)
        {
            var time = TimeSpan.Zero;
            if (args.Args is { Count: > 0 } && args.Args[0].Value is TimeSpan timeArg)
            {
                time = timeArg;
            }
            return new LocValueString(FormatPlaytime(time));
        }
    }
}