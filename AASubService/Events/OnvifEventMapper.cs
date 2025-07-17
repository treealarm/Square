using EventServiceReference1;
using Google.Protobuf.WellKnownTypes;
using LeafletAlarmsGrpc;

namespace AASubService.Events
{
  public static class OnvifEventMapper
  {
    public static EventProto Map(NotificationMessageHolderType msg, string ObjectId)
    {
      var evt = new EventProto
      {
        Timestamp = Timestamp.FromDateTime(DateTime.UtcNow),
        EventName = msg.Topic?.Any?.FirstOrDefault()?.InnerText ?? "Unknown ONVIF Event",
        EventPriority = (int)LogLevel.Information,
        Param0 = "1",
        Param1 = "2"
      };

      evt.ExtraProps.Add(new ProtoObjExtraProperty
      {
        PropName = "raw_xml",
        StrVal = msg.Message?.OuterXml ?? "<empty>"
      });

      if (msg.Message?.FirstChild is System.Xml.XmlElement el)
      {
        foreach (System.Xml.XmlNode node in el.ChildNodes)
        {
          if (node is System.Xml.XmlElement childEl)
          {
            evt.ExtraProps.Add(new ProtoObjExtraProperty
            {
              PropName = childEl.LocalName,
              StrVal = childEl.InnerText
            });
          }
        }
      }

      evt.ObjectId = ObjectId;

      return evt;
    }
  }

}
