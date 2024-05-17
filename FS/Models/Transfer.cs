using System.Diagnostics;
using FS.Utilities;

namespace FS.Models;

 using System.Collections.Generic;
 using System.Text.Json;
 using System.Text.Json.Serialization;
 using System.Globalization;

 public partial class Transfer
 {
     [JsonPropertyName("id")]
     public int Id { get; set; }

     [JsonPropertyName("userid")]
     public int Userid { get; set; }

     [JsonPropertyName("user_email")]
     public string UserEmail { get; set; }

     [JsonPropertyName("subject")]
     public string? Subject { get; set; }

     [JsonPropertyName("message")]
     public string? Message { get; set; }

     [JsonPropertyName("options")]
     public IDictionary<string,bool>? Options { get; set; }

     [JsonPropertyName("salt")]
     public string Salt { get; set; }

     [JsonPropertyName("roundtriptoken")]
     public string Roundtriptoken { get; set; }

     [JsonPropertyName("files")]
     public TransferFile[] Files { get; set; }

     [JsonPropertyName("recipients")]
     public Recipient[] Recipients { get; set; }
    
     [JsonPropertyName("created")]
     public FileSenderTime Created { get; set; }
     
     [JsonPropertyName("expires")]
     public FileSenderTime Expiry { get; set; }
     public String ViewRecipients
     {
         get
         {
             return string.Join(", ", Recipients.Select(x => x.Email));;
         }
     }

     public String ViewFiles
     {
         get
         {
             var tmp = string.Join("\n", Files.Take(3).Select(x => x.Name));
             if (Files.Length > 3)
             {
                 tmp += Files.Length == 4 ? $"\n{Files[3].Name}" : "\n....";
             }
             return  tmp;
         }
     }
 }

 public partial class TransferFile
 {
     [JsonPropertyName("id")]
     public int Id { get; set; }

     [JsonPropertyName("uid")]
     public Guid Uid { get; set; }

     [JsonPropertyName("name")]
     public string Name { get; set; }

     [JsonPropertyName("size")]
     [JsonConverter(typeof(CustomLongConverter))]

     public long Size { get; set; }

     public string HumanSize => FileSize.getHumanFileSize(Size);

     [JsonPropertyName("cid")]
     public String? Cid { get; set; }
 }

 public partial class FileSenderTime 
 {
     [JsonPropertyName("raw")]
     [JsonConverter(typeof(CustomLongConverter))]
     public long? UnixTime { get; set; }
     
     [JsonPropertyName("formatted")]
     public string? FormatedDate { get; set; }

 }

 public partial class Recipient
 {
     [JsonPropertyName("id")]
     public long Id { get; set; }

     [JsonPropertyName("transfer_id")]
     public long TransferId { get; set; }

     [JsonPropertyName("token")]
     public Guid Token { get; set; }

     [JsonPropertyName("email")]
     public string Email { get; set; }


     /*[JsonPropertyName("last_activity")]
     public object LastActivity { get; set; }*/

     /*
     [JsonPropertyName("options")]
     public object Options { get; set; }
     */

     [JsonPropertyName("download_url")]
     public Uri DownloadUrl { get; set; }

     /*[JsonPropertyName("errors")]
     public object[] Errors { get; set; }*/
 }

 public partial class Transfer
 {
     public static Transfer FromJson(string json) => JsonSerializer.Deserialize<Transfer>(json, Converter.Settings);
 }

 public static class Serialize
 {
     public static string ToJson(this Transfer self) => JsonSerializer.Serialize(self, Converter.Settings);
 }

 internal static class Converter
 {
     public static readonly JsonSerializerOptions Settings = new(JsonSerializerDefaults.General)
     {
         Converters =
         {
             new DateOnlyConverter(),
             new TimeOnlyConverter(),
             IsoDateTimeOffsetConverter.Singleton
         },
     };
 }
 class CustomLongConverter : JsonConverter<long>
 {
     //The transfer api endpoint gives me a number but the create transfer endpoint gives me a string....
     public override long Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
     {
         if (reader.TokenType == JsonTokenType.String)
         {
             if (long.TryParse(reader.GetString(), out long result))
             {
                 return result;
             }
             else
             {
                 throw new JsonException($"Unable to parse '{reader.GetString()}' as long.");
             }
         }
         else if (reader.TokenType == JsonTokenType.Number)
         {
             return reader.GetInt64();
         }
         else
         {
             throw new JsonException($"Unexpected token type: {reader.TokenType}");
         }
     }

     public override void Write(Utf8JsonWriter writer, long value, JsonSerializerOptions options)
     {
         writer.WriteStringValue(value.ToString());
     }
 }
 
 public class DateOnlyConverter : JsonConverter<DateOnly>
 {
     private readonly string serializationFormat;
     public DateOnlyConverter() : this(null) { }

     public DateOnlyConverter(string? serializationFormat)
     {
         this.serializationFormat = serializationFormat ?? "yyyy-MM-dd";
     }

     public override DateOnly Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
     {
         var value = reader.GetString();
         return DateOnly.Parse(value!);
     }

     public override void Write(Utf8JsonWriter writer, DateOnly value, JsonSerializerOptions options)
         => writer.WriteStringValue(value.ToString(serializationFormat));
 }

 public class TimeOnlyConverter : JsonConverter<TimeOnly>
 {
     private readonly string serializationFormat;

     public TimeOnlyConverter() : this(null) { }

     public TimeOnlyConverter(string? serializationFormat)
     {
         this.serializationFormat = serializationFormat ?? "HH:mm:ss.fff";
     }

     public override TimeOnly Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
     {
         var value = reader.GetString();
         return TimeOnly.Parse(value!);
     }

     public override void Write(Utf8JsonWriter writer, TimeOnly value, JsonSerializerOptions options)
         => writer.WriteStringValue(value.ToString(serializationFormat));
 }

 internal class IsoDateTimeOffsetConverter : JsonConverter<DateTimeOffset>
 {
     public override bool CanConvert(Type t) => t == typeof(DateTimeOffset);

     private const string DefaultDateTimeFormat = "yyyy'-'MM'-'dd'T'HH':'mm':'ss.FFFFFFFK";

     private DateTimeStyles _dateTimeStyles = DateTimeStyles.RoundtripKind;
     private string? _dateTimeFormat;
     private CultureInfo? _culture;

     public DateTimeStyles DateTimeStyles
     {
         get => _dateTimeStyles;
         set => _dateTimeStyles = value;
     }

     public string? DateTimeFormat
     {
         get => _dateTimeFormat ?? string.Empty;
         set => _dateTimeFormat = (string.IsNullOrEmpty(value)) ? null : value;
     }

     public CultureInfo Culture
     {
         get => _culture ?? CultureInfo.CurrentCulture;
         set => _culture = value;
     }

     public override void Write(Utf8JsonWriter writer, DateTimeOffset value, JsonSerializerOptions options)
     {
         string text;


         if ((_dateTimeStyles & DateTimeStyles.AdjustToUniversal) == DateTimeStyles.AdjustToUniversal
             || (_dateTimeStyles & DateTimeStyles.AssumeUniversal) == DateTimeStyles.AssumeUniversal)
         {
             value = value.ToUniversalTime();
         }

         text = value.ToString(_dateTimeFormat ?? DefaultDateTimeFormat, Culture);

         writer.WriteStringValue(text);
     }

     public override DateTimeOffset Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
     {
         string? dateText = reader.GetString();

         if (string.IsNullOrEmpty(dateText) == false)
         {
             if (!string.IsNullOrEmpty(_dateTimeFormat))
             {
                 return DateTimeOffset.ParseExact(dateText, _dateTimeFormat, Culture, _dateTimeStyles);
             }
             else
             {
                 return DateTimeOffset.Parse(dateText, Culture, _dateTimeStyles);
             }
         }
         else
         {
             return default(DateTimeOffset);
         }
     }


     public static readonly IsoDateTimeOffsetConverter Singleton = new IsoDateTimeOffsetConverter();
 } 