namespace FS.Models;

 using System.Collections.Generic;
 using System.Text.Json;
 using System.Text.Json.Serialization;
 using System.Globalization;

 public partial class Transfer
 {
     [JsonPropertyName("id")]
     public long Id { get; set; }

     [JsonPropertyName("userid")]
     public long Userid { get; set; }

     [JsonPropertyName("user_email")]
     public string UserEmail { get; set; }

     [JsonPropertyName("subject")]
     public string Subject { get; set; }

     [JsonPropertyName("message")]
     public string Message { get; set; }

     [JsonPropertyName("created")]
     public Created Created { get; set; }

     [JsonPropertyName("expires")]
     public Expires Expires { get; set; }

     [JsonPropertyName("expiry_date_extension")]
     public long ExpiryDateExtension { get; set; }

     [JsonPropertyName("options")]
     public Options Options { get; set; }

     [JsonPropertyName("salt")]
     public string Salt { get; set; }

     [JsonPropertyName("roundtriptoken")]
     public string Roundtriptoken { get; set; }

     [JsonPropertyName("files")]
     public Transfer_File[] Files { get; set; }

     [JsonPropertyName("recipients")]
     public Recipient[] Recipients { get; set; }
 }

 public partial class Created
 {
     [JsonPropertyName("raw")]
     public long Raw { get; set; }

     [JsonPropertyName("formatted")]
     public string Formatted { get; set; }
 }

 public partial class Expires
 {
     [JsonPropertyName("raw")]
     [JsonConverter(typeof(ParseStringConverter))]
     public long Raw { get; set; }

     [JsonPropertyName("formatted")]
     public string Formatted { get; set; }
 }

 public partial class Transfer_File
 {
     [JsonPropertyName("id")]
     public long Id { get; set; }

     [JsonPropertyName("transfer_id")]
     public long TransferId { get; set; }

     [JsonPropertyName("uid")]
     public Guid Uid { get; set; }

     [JsonPropertyName("name")]
     public string Name { get; set; }

     [JsonPropertyName("size")]
     [JsonConverter(typeof(ParseStringConverter))]
     public long Size { get; set; }

     [JsonPropertyName("sha1")]
     public object Sha1 { get; set; }

     [JsonPropertyName("cid")]
     public Guid Cid { get; set; }
 }

 public partial class Options
 {
     [JsonPropertyName("get_a_link")]
     public bool GetALink { get; set; }

     [JsonPropertyName("add_me_to_recipients")]
     public bool AddMeToRecipients { get; set; }

     [JsonPropertyName("email_recipient_when_transfer_expires")]
     public bool EmailRecipientWhenTransferExpires { get; set; }

     [JsonPropertyName("hide_sender_email")]
     public bool HideSenderEmail { get; set; }

     [JsonPropertyName("encryption")]
     public bool Encryption { get; set; }
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

     [JsonPropertyName("created")]
     public Created Created { get; set; }

     [JsonPropertyName("last_activity")]
     public object LastActivity { get; set; }

     [JsonPropertyName("options")]
     public object Options { get; set; }

     [JsonPropertyName("download_url")]
     public Uri DownloadUrl { get; set; }

     [JsonPropertyName("errors")]
     public object[] Errors { get; set; }
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

 internal class ParseStringConverter : JsonConverter<long>
 {
     public override bool CanConvert(Type t) => t == typeof(long);

     public override long Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
     {
         var value = reader.GetString();
         long l;
         if (Int64.TryParse(value, out l))
         {
             return l;
         }
         throw new Exception("Cannot unmarshal type long");
     }

     public override void Write(Utf8JsonWriter writer, long value, JsonSerializerOptions options)
     {
         JsonSerializer.Serialize(writer, value.ToString(), options);
         return;
     }

     public static readonly ParseStringConverter Singleton = new ParseStringConverter();
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