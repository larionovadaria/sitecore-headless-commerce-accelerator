'use strict';

const path = require('path');
const autoprefixer = require('autoprefixer');
const ForkTsCheckerWebpackPlugin = require('fork-ts-checker-webpack-plugin');
const MiniCssExtractPlugin = require('mini-css-extract-plugin');
const CleanWebpackPlugin = require('clean-webpack-plugin');
const webpack = require('webpack');

const constants = require('./constants');
// Preparation
const root = './../../..';

const resolve = {
  extensions: ['.tsx', '.ts', '.js', '.css', '.scss'],
  alias: {
    'data-api-alias': path.resolve(process.cwd(), constants.jssDataApiImplementationProdPath),
    Foundation: path.resolve(process.cwd(), './Foundation/'),
    Project: path.resolve(process.cwd(), './Project/'),
    Feature: path.resolve(process.cwd(), './Feature/'),
  },
};

const cleanWebpackPluginOptions = {
  root: path.resolve(__dirname, root),
};

const extractSass = new MiniCssExtractPlugin({
  filename: '[name].css',
  chunkFilename: '[id].css',
});

const clientWebpackConfigFactory = (projectManifest) => {
  const { name, clientEntry, outputPath } = projectManifest;
  const { apiKey } = projectManifest.env[0];

  const clientEntryPath = `./Project/${name}/client/${clientEntry}`;
  const clientOutputPath = `${root}/Project/${name}/client/${outputPath}`;

  return {
    mode: 'production',
    target: 'web',
    devtool: 'eval-source-map',
    entry: {
      client: clientEntryPath,
    },
    output: {
      path: path.resolve(__dirname, clientOutputPath),
      // wooli is hadcoded here, because we have to specify the solution name
      // TODO: remove hardcoded solution name from the publicPath
      publicPath: `/dist/wooli/project/${name.toLowerCase()}`,
      filename: '[name].bundle.js',
    },
    optimization: {
      minimize: true,
      splitChunks: {
        chunks: 'all',
        name: true,
        cacheGroups: {
          vendors: {
            name: 'vendors',
            filename: '[name].bundle.js',
            test: /[\\/]node_modules[\\/]/,
          },
          client: {
            name: 'client',
            filename: '[name].bundle.js',
            test: /[\\/](Foundation|Feature|Project)[\\/]/,
          },
        },
      },
    },
    resolve,
    module: {
      rules: [
        {
          test: [/\.jpe?g$/, /\.png$/, /\.svg$/, /\.ttf$/, /\.otf$/, /\.eot/, /\.woff/],
          loader: require.resolve('url-loader'),
          options: {
            limit: 8192,
            name: '[name].[ext]',
            outputPath: '/',
          },
        },
        {
          test: /\.(ts|tsx)$/,
          exclude: /node_modules/,
          use: [
            {
              loader: require.resolve('ts-loader'),
              options: {
                // disable type checker - we will use it in fork plugin
                transpileOnly: true,
              },
            },
          ],
        },
        {
          test: /\.scss$/,
          use: [
            {
              loader: MiniCssExtractPlugin.loader,
            },
            {
              loader: 'css-loader',
              options: {
                minimize: {
                  // cssnano
                  preset: 'default',
                },
              },
            },
            {
              loader: require.resolve('postcss-loader'),
              options: {
                // Necessary for external CSS imports to work
                // https://github.com/facebookincubator/create-react-app/issues/2677
                ident: 'postcss',
                plugins: () => [
                  require('postcss-flexbugs-fixes'),
                  require('postcss-object-fit-images'),
                  autoprefixer({
                    browsers: [
                      '>1%',
                      'last 4 versions',
                      'Firefox ESR',
                      'not ie < 9', // React doesn't support IE8 anyway
                    ],
                    flexbox: 'no-2009',
                  }),
                ],
              },
            },
            {
              loader: require.resolve('sass-loader'),
            },
          ],
        },
      ],
    },
    plugins: [
      new ForkTsCheckerWebpackPlugin({
        async: false,
        include: ['/Project/', '/Feature/', '/Foundation/'],
        tsconfig: './tsconfig.json',
        tslint: './tslint.json',
      }),
      new webpack.DefinePlugin({
        'process.env.API_KEY': JSON.stringify(apiKey),
      }),
      extractSass,
    ],
  };
};

const serverWebpackConfigFactory = (projectManifest) => {
  const { name, serverEntry, outputPath, publicFolderPath } = projectManifest;

  const serverEntryPath = `./Project/${name}/client/${serverEntry}`;
  const serverOutputPath = `${root}/Project/${name}/client/${outputPath}`;
  return {
    mode: 'production',
    target: 'node',
    entry: {
      server: serverEntryPath,
    },
    output: {
      path: path.resolve(__dirname, serverOutputPath),
      filename: '[name].bundle.js',
      libraryTarget: 'this', // this option is required for use with JavaScriptViewEngine
    },
    optimization: {
      minimize: false,
    },
    resolve,
    module: {
      rules: [
        {
          test: [/\.jpe?g$/, /\.png$/, /\.svg$/, /\.ttf$/, /\.otf$/, /\.eot/, /\.woff/],
          loader: require.resolve('url-loader'),
          options: {
            limit: 8192,
            name: '[name].[ext]',
            useRelativePath: false,
          },
        },
        {
          test: /\.(ts|tsx)$/,
          exclude: /node_modules/,
          use: [
            {
              loader: require.resolve('ts-loader'),
              options: {
                // disable type checker - we will use it in fork plugin
                transpileOnly: true,
              },
            },
          ],
        },
        {
          test: /\.html$/,
          exclude: /node_modules/,
          use: { loader: 'html-loader' },
        },
        { test: /\.scss$/, loader: 'ignore-loader' },
      ],
    },
    plugins: [
      new ForkTsCheckerWebpackPlugin({
        async: false,
        include: ['/Project/', '/Feature/', '/Foundation/'],
        tsconfig: './tsconfig.json',
        tslint: './tslint.json',
      }),
    ],
  };
};

module.exports = (projectManifests) =>
  projectManifests.reduce((configs, manifest) => {
    configs.push(clientWebpackConfigFactory(manifest));
    configs.push(serverWebpackConfigFactory(manifest));

    return configs;
  }, []);
