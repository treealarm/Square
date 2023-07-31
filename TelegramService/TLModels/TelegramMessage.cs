using System.Collections.Generic;

namespace TelegramService
{
  public class InlineKeyboardButton
  {
    public string text { get; set; }
    public string callback_data { get; set; }
  }

  public class KeyboardButton
  {
    public string text { get; set; }
    public bool request_location { get; set; } = true;
  }

  public class LocationMarkup
  {
    public bool one_time_keyboard { get; set; } = true;
    public List<List<KeyboardButton>> keyboard { get; set; }
  }

  public class TelegramReplyMarkup
  {    public List<List<InlineKeyboardButton>> inline_keyboard { get; set; }
  }

  public class TelegramMessage
  {
    public string bot_id { get; set; }
    public string chat_id { get; set; }
    public string proxy { get; set; }
    public string latitude { get; set; }
    public string longitude { get; set; }
    public string text { get; set; }
    public string photo { get; set; }
    public object reply_markup { get; set; }
  }
}