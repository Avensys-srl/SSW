using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace SSW
{
    internal static class CLOfferArchive
    {
        internal sealed class Entry
        {
            public string Path;
            public CLSelectionProjectDocument Document;
            public string[] Paths;
        }

        internal static string DirectoryPath
        {
            get
            {
                string configured = Environment.GetEnvironmentVariable("SSW_OFFER_ARCHIVE_PATH");
                return String.IsNullOrWhiteSpace(configured)
                    ? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                        "Avensys", "SSW", "FollowUps", "Selections")
                    : Path.GetFullPath(configured);
            }
        }

        internal static List<Entry> Read()
        {
            var entries = new List<Entry>();
            if (!Directory.Exists(DirectoryPath)) return entries;
            foreach (string path in Directory.EnumerateFiles(DirectoryPath, "*" + CLSelectionProjectSerializer.FileExtension))
            {
                try
                {
                    var document = CLSelectionProjectSerializer.Load(path);
                    var identity = document.Identity;
                    if (identity == null || identity.OfferStatus != "Definitive" ||
                        !identity.Revision.HasValue || identity.OfferRevision != identity.Revision ||
                        String.IsNullOrWhiteSpace(identity.PublicReference)) continue;
                    entries.Add(new Entry { Path = path, Document = document });
                }
                catch (Exception error)
                {
                    Trace.TraceWarning("Offer archive could not read {0}: {1}", path, error.Message);
                }
            }
            return entries.GroupBy(entry => entry.Document.Identity.PublicReference + ":" + entry.Document.Identity.Revision)
                .Select(group =>
                {
                    var entry = group.OrderByDescending(item => File.GetLastWriteTimeUtc(item.Path)).First();
                    entry.Paths = group.Select(item => item.Path).ToArray();
                    return entry;
                })
                .OrderByDescending(entry => entry.Document.Identity.OfferDefinitiveAtUtc ?? File.GetLastWriteTimeUtc(entry.Path))
                .ToList();
        }
    }
}
