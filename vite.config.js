import react from "@vitejs/plugin-react";
import mkcert from 'vite-plugin-mkcert'
import { defineConfig } from 'vite'
import { resolve } from 'path'

var clientPort = process.env.CLIENT_PORT == null ? 8080 : parseInt(process.env.CLIENT_PORT);
var serverPort = process.env.SERVER_PORT == null ? 8085 : parseInt(process.env.SERVER_PORT);
serverPort = process.env.SERVER_PROXY_PORT == null ? serverPort : parseInt(process.env.SERVER_PROXY_PORT);

var certDir = `${process.env.HOME}/.vite-plugin-mkcert`;

var proxy = {
  target: `http://127.0.0.1:${serverPort}/`,
  changeOrigin: false,
  secure: false,
  ws: true
}

export default defineConfig({
    server: {
        port: clientPort,
        host: '0.0.0.0',
        https: false,
        cors: true,
        proxy: {
            '/api': proxy,
            '/isAuthenticated': proxy,
            '/signin-oidc': proxy,
            '/signin': proxy,
            '/signout': proxy,
            '/token': proxy,
            '/claims': proxy,
            '/hub': proxy,
            '/socket': proxy,
        }
    },
    plugins: [
        react({jsxRuntime: "automatic"}),
        mkcert({
            hosts: [
                "localhost",
                "*.local.oceanbox.io"
            ],
            savePath: `${certDir}/certs`,
            mkcertPath: `${certDir}/mkcert`
        })
    ]
})