using System.Collections.Generic;

namespace TelegramService
{  
  public class CallbackQuery
  {
    public string id { get; set; }
    public From from { get; set; }
    public Message message { get; set; }    
    public string chat_instance { get; set; }
    public string data { get; set; }
  }

  public class Chat
  {
    public long id { get; set; }
    public string title { get; set; }
    public string type { get; set; }
    public bool all_members_are_administrators { get; set; }
  }

  public class Entity
  {
    public long offset { get; set; }
    public long length { get; set; }
    public string type { get; set; }
  }

  public class From
  {
    public long id { get; set; }
    public bool is_bot { get; set; }
    public string first_name { get; set; }
    public string last_name { get; set; }
    public string username { get; set; }
    public string language_code { get; set; }
  }

  public class Photo
  {
    public string file_id { get; set; }
    public string file_unique_id { get; set; }
    public int file_size { get; set; }
    public int width { get; set; }
    public int height { get; set; }
  }

  public class Location
  {
    public double latitude { get; set; }
    public double longitude { get; set; }
    public int live_period { get; set; }
    public int heading { get; set; }
    public double horizontal_accuracy { get; set; }
  }

  public class Message
  {
    public long message_id { get; set; }
    public From from { get; set; }
    public Chat chat { get; set; }
    public long? date { get; set; }
    public long? edit_date { get; set; }
    
    public string text { get; set; }
    public Location location { get; set; }
    public List<Entity> entities { get; set; }
    public ReplyMarkup reply_markup { get; set; }
    public List<Photo> photo { get; set; }
    public Message reply_to_message { get; set; }
  }

  public class ReplyMarkup
  {
    public List<List<dynamic>> inline_keyboard { get; set; }
  }

  public class Result
  {
    public long update_id { get; set; }
    public CallbackQuery callback_query { get; set; }
    public Message message { get; set; }
    public Message edited_message { get; set; }
  }

  public class TelegramUpdate
  {
    public bool ok { get; set; }
    public List<Result> result { get; set; }
  }

  public class SingleResult
  {
    public int message_id { get; set; }
    public From from { get; set; }
    public Chat chat { get; set; }
    public int date { get; set; }
    public string text { get; set; }
  }

  public class TelegramSingleUpdate
  {
    public bool ok { get; set; }
    public SingleResult result { get; set; }
  }

  public class FileResult
  {
    public string file_id { get; set; }
    public string file_unique_id { get; set; }
    public int file_size { get; set; }
    public string file_path { get; set; }
  }

  public class GetFileResponse
  {
    public bool ok { get; set; }
    public FileResult result { get; set; }
  }
}