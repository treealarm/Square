﻿using DbLayer.Models;
using Domain;
using Domain.GeoDBDTO;
using Domain.GeoDTO;
using Domain.ServiceInterfaces;
using Domain.StateWebSock;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using MongoDB.Driver.GeoJsonObjectModel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbLayer.Services
{
    public class TrackService : ITrackService
    {
        private readonly IMongoCollection<DBTrackPoint> _collFigures;
        private readonly MongoClient _mongoClient;
        private readonly ILevelService _levelService;
        public TrackService(
          IOptions<MapDatabaseSettings> geoStoreDatabaseSettings,
          ILevelService levelService
        )
        {
            _mongoClient = new MongoClient(
              geoStoreDatabaseSettings.Value.ConnectionString);

            var mongoDatabase = _mongoClient.GetDatabase(
                geoStoreDatabaseSettings.Value.DatabaseName);

            _collFigures =
              mongoDatabase.GetCollection<DBTrackPoint>(
                geoStoreDatabaseSettings.Value.TracksCollectionName
              );
            _levelService = levelService;

            CreateIndexes();
        }

        private void CreateIndexes()
        {
            IndexKeysDefinition<DBTrackPoint> keys =
              new IndexKeysDefinitionBuilder<DBTrackPoint>()
              .Ascending(d => d.figure.zoom_level)
              .Geo2DSphere(d => d.figure.location)
              .Ascending(d => d.timestamp)

              ;

            var indexModel = new CreateIndexModel<DBTrackPoint>(
              keys, new CreateIndexOptions()
              { Name = "location" }
            );

            _collFigures.Indexes.CreateOneAsync(indexModel);
        }
        public async Task<List<TrackPointDTO>> InsertManyAsync(List<TrackPointDTO> newObjs)
        {
            List<DBTrackPoint> list = new List<DBTrackPoint>();

            foreach (var track in newObjs)
            {
                var dbTrack = new DBTrackPoint()
                {
                    timestamp = track.timestamp,
                    figure = ModelGate.ConvertDTO2DB(track.figure)
                };
                list.Add(dbTrack);
            }

            if (list.Count > 0)
            {
                await _collFigures.InsertManyAsync(list);
            }

            return DBListToDTO(list);
        }

        private TrackPointDTO ConvertDB2DTO(DBTrackPoint t)
        {
            if (t == null)
            {
                return null;
            }

            var dto = new TrackPointDTO()
            {
                id = t.id,
                timestamp = t.timestamp,
                figure = ModelGate.ConvertDB2DTO(t.figure)
            };

            return dto;
        }

        public async Task<TrackPointDTO> GetLastAsync(string figure_id, string ignoreTrackId)
        {
            var dbTrack =
              await _collFigures
                .Find(t => t.figure.id == figure_id && t.id != ignoreTrackId)
                .SortByDescending(t => t.id)
                .FirstOrDefaultAsync();

            return ConvertDB2DTO(dbTrack);
        }

        private List<TrackPointDTO> DBListToDTO(List<DBTrackPoint> dbTracks)
        {
            List<TrackPointDTO> list = new List<TrackPointDTO>();
            foreach (var t in dbTracks)
            {
                var dto = ConvertDB2DTO(t);
                list.Add(dto);
            }
            return list;
        }

        private static void Log(FilterDefinition<BsonDocument> filter)
        {
            var serializerRegistry = BsonSerializer.SerializerRegistry;
            var documentSerializer = serializerRegistry.GetSerializer<BsonDocument>();
            var rendered = filter.Render(documentSerializer, serializerRegistry);
            Debug.WriteLine(rendered.ToJson(new JsonWriterSettings { Indent = true }));
            Debug.WriteLine("");
        }

        public async Task<List<TrackPointDTO>> GetTracksByBox(BoxTrackDTO box)
        {
            //List<ObjectId> ids = new List<ObjectId>();
            //ids.Add(new ObjectId("6310c598131d54f2b292b61f"));
            //var filter1 = Builders<BsonDocument>.Filter.In("figure._id", ids);
            //Log(filter1);


            var builder = Builders<DBTrackPoint>.Filter;
            var geometry = GeoJson.Polygon(
              new GeoJson2DCoordinates[]
              {
          GeoJson.Position(box.wn[0], box.wn[1]),
          GeoJson.Position(box.es[0], box.wn[1]),
          GeoJson.Position(box.es[0], box.es[1]),
          GeoJson.Position(box.wn[0], box.es[1]),
          GeoJson.Position(box.wn[0], box.wn[1])
              }
            );

            FilterDefinition<DBTrackPoint> filter = FilterDefinition<DBTrackPoint>.Empty;

            if (box.zoom != null)
            {
                var levels = await _levelService.GetLevelsByZoom(box.zoom);

                filter =
                  builder.Where(p => levels.Contains(p.figure.zoom_level)
                  || p.figure.zoom_level == null);
            }

            if (box.time_start != null)
            {
                filter = filter & builder.Where(t => t.timestamp >= box.time_start);
            }

            if (box.time_end != null)
            {
                filter = filter & builder.Where(t => t.timestamp <= box.time_end);
            }

            if (box.ids != null)
            {
                filter = filter & builder.Where(t => box.ids.Contains(t.figure.id));
            }

            filter = filter & builder.GeoIntersects(t => t.figure.location, geometry);


            var dbTracks = await _collFigures.Find(filter).ToListAsync();


            return DBListToDTO(dbTracks);
        }
    }
}