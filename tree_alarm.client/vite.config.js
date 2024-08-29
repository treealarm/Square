import { defineConfig } from 'vite';
import plugin from '@vitejs/plugin-react';
import { fileURLToPath, URL } from 'node:url';
//import fs from 'fs';
import path from 'path';
//import child_process from 'child_process';
import { env } from 'process';
import eslintPlugin from 'vite-plugin-eslint'

// �������� ���������� ���� � �������� �����
const __dirname = fileURLToPath(new URL('.', import.meta.url));

// ���� � ����� `src`
const srcPath = path.resolve(__dirname, './src');

// ���� � ����� `node_modules`
const nodeModulesPath = path.resolve(__dirname, './node_modules');

//// ����� ��� �������� ������������
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

// ���������� ������� URL
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
      '@': srcPath, // ��������� @ ��������� �� ����� src
      '~': nodeModulesPath // ��������� ~ ��������� �� ����� node_modules
    }
  },
  server: {
    proxy: {
      '^/swagger': { target, secure: false },
      '^/api': { target, secure: false },
      '^/public': { target, secure: false },
      '^/static_files': { target, secure: false },
      // ������������� ��� WebSocket
      '^/state': {
        target,
        // ��������� ������, ����� middleware �� ��������, ����� ������ ASP.NET Core ����������
        onError: (err, req, res) => {
          console.error(`${err.message}`, req.url);
          res.writeHead(500, {
            'Content-Type': 'text/plain'
          });
          res.end('Proxy error');
        },
        secure: false,
        ws: true,
        // ���������� ���-�����-������� ��� ���������
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
