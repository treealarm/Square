import { defineConfig } from 'vite';
import plugin from '@vitejs/plugin-react';
import { fileURLToPath, URL } from 'node:url';
//import fs from 'fs';
import path from 'path';
//import child_process from 'child_process';
import { env } from 'process';
import eslintPlugin from 'vite-plugin-eslint'

// Получаем абсолютный путь к текущему файлу
const __dirname = fileURLToPath(new URL('.', import.meta.url));

// Путь к папке `src`
const srcPath = path.resolve(__dirname, './src');

// Путь к папке `node_modules`
const nodeModulesPath = path.resolve(__dirname, './node_modules');

//// Папка для хранения сертификатов
//const baseFolder =
//  env.APPDATA !== undefined && env.APPDATA !== ''
//    ? `${env.APPDATA}/ASP.NET/https`
//    : `${env.HOME}/.aspnet/https`;

//const certificateName = "tree_alarm.client";
//const certFilePath = path.join(baseFolder, `${certificateName}.pem`);
//const keyFilePath = path.join(baseFolder, `${certificateName}.key`);

//if (!fs.existsSync(certFilePath) || !fs.existsSync(keyFilePath)) {
//  if (0 !== child_process.spawnSync('dotnet', [
//    'dev-certs',
//    'https',
//    '--export-path',
//    certFilePath,
//    '--format',
//    'Pem',
//    '--no-password',
//  ], { stdio: 'inherit', }).status) {
//    throw new Error("Could not create certificate.");
//  }
//}

// Определяем целевую URL
const target = env.ASPNETCORE_URLS ? env.ASPNETCORE_URLS.split(';')[0] : 'http://localhost:8000';

export default defineConfig({
  plugins: [
    plugin(),
    eslintPlugin({
      cache: false,
      include: ['./src/**/*.js', './src/**/*.jsx', './src/**/*.tsx'],
      exclude: [],
    }),
  ],
  resolve: {
    alias: {
      '@': srcPath, // Псевдоним @ указывает на папку src
      '~': nodeModulesPath // Псевдоним ~ указывает на папку node_modules
    }
  },
  server: {
    proxy: {
      '^/swagger': { target, secure: false },
      '^/api': { target, secure: false },
      '^/public': { target, secure: false },
      '^/static_files': { target, secure: false },
      // Проксирование для WebSocket
      '^/state': {
        target,
        // Обработка ошибок, чтобы middleware не крашился, когда сервер ASP.NET Core недоступен
        onError: (err, req, res) => {
          console.error(`${err.message}`, req.url);
          res.writeHead(500, {
            'Content-Type': 'text/plain'
          });
          res.end('Proxy error');
        },
        secure: false,
        ws: true,
        // Проксируем веб-сокет-запросы без изменений
        onProxyReqWs: (proxyReq, req) => {
          proxyReq.setHeader('Host', req.headers.host);
        }
      }
    },
    port: 8002
    //,
    //https: {
    //  key: fs.readFileSync(keyFilePath),
    //  cert: fs.readFileSync(certFilePath)
    //}
  }
});
