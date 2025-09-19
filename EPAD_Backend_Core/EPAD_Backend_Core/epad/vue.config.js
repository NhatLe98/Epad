const path = require('path');
const webpack = require('webpack');
const CopyWebpackPlugin = require('copy-webpack-plugin');
const jsonminify = require('jsonminify');
const production = 'production' === process.env.NODE_ENV;
const misc = path.resolve(path.join(__dirname, 'src/$core/misc'));
function resolve(dir) {
  return path.join(__dirname, dir);
}
module.exports = {
  assetsDir: 'assets',
  productionSourceMap: false,
  configureWebpack: {
    node: {
      fs: 'empty'
    },
    resolve: {
      alias: {
        '@': resolve('src')
      }
    },
    performance: {
      hints: false
    },
    optimization: {
      splitChunks: {
        chunks: 'all',
      }
    },
    module: {
      rules: [
        {
          test: /config.*config\.js$/,
          use: [
            {
              loader: 'file-loader',
              options: {
                name: 'config.js'
              },
            }
          ]
        }
      ]
    },
    plugins: [
      new CopyWebpackPlugin([
        {
          from: resolve('src/static'),
          to: resolve('dist/static'),
          transform(fileContent, path) {
            let pattJSON = /\.json$/gi; // filter json file
            if (pattJSON.test(path)) {
              if (production) {
                return jsonminify(fileContent.toString());
              }
              return fileContent;
            }
            return fileContent;
          }
        },
        {
          from: resolve('src/static'),
          to: resolve('dist/static'),
          transform(fileContent, path) {
            let pathJSCfg = /config.js$/gi; // filter config file
            if (pathJSCfg.test(path)) {
              return fileContent;
            }
            return fileContent;
          }
        }
      ]),
      new webpack.ProvidePlugin({
        Misc: misc,
        moment: 'moment'
      })
    ]
  },
  pluginOptions: {
    i18n: {
      locale: 'en',
      fallbackLocale: 'en',
      localeDir: 'locales',
      enableInSFC: false
    }
  }
}
