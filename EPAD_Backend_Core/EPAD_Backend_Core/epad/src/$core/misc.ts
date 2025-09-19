import axios from 'axios';
import { resolve } from 'path';
import * as utils from './misc';
import * as mimeTypes from 'mime-types';

declare global {
  const Misc: typeof utils;
}

/**
 * Tạo AxiosInstance
 */
function createHttp() {
  return axios.create({
    headers: { 'Access-Control-Allow-Origin': '*', 'Cache-Control': 'no-cache' }
  });
}

/**
 * Đọc file theo đường dẫn
 * @param path
 */
export async function readFile(path: string) {
  return await createHttp().get(resolve('..', path));
}

/**
 * Đọc file theo đường dẫn
 * @param path
 */
export function readFileAsync(path: string) {
  const data = createHttp()
    .get(resolve('..', path))
    .then(e => e.data)
    .catch(error => {
      throw error;
    });
  return data;
}

/**
 * Kiểm tra chuỗi rỗng
 * @param value
 */
export function isEmpty(value: any) {
  return isNullOrUndefined(value) || value === '' || value.length === 0;
}

export function isNumber(value: any) {
  return /^\d+$/.test(value);
}

export function toLowerFirstCase(str: string) {
  if (isEmpty(str)) {
    return '';
  }
  const strFirst = str.slice(0, 1);
  str = str.substr(1);
  return `${strFirst.toLowerCase()}${str}`;
}

export function camelToSnakeCase(str: string, split = '-') {
  return toLowerFirstCase(str).replace(
    /[A-Z]/g,
    letter => `${split}${letter.toLowerCase()}`
  );
}

export function kebabCase2PascalCase(str: string){
  str +='';
  const words = str.split('-');
  return words.map(word => {
    return word.slice(0,1).toUpperCase() + word.slice(1, word.length).toLowerCase();
  }).join('');
}

/**
 * Simple is object check.
 * @param item
 * @returns {boolean}
 */
function isObject(item) {
  return (item && typeof item === 'object' && !Array.isArray(item) && item !== null);
}


/**
* Deep merge two objects.
* @param target
* @param source
*/
export function mergeDeep(target, source) {
  if (isObject(target) && isObject(source)) {
      Object.keys(source).forEach(key => {
          if (isObject(source[key])) {
              if (!target[key] || !isObject(target[key])) {
                  target[key] = source[key];
              }
              mergeDeep(target[key], source[key]);
          } else {
              Object.assign(target, { [key]: source[key] });
          }
      });
  }
  return target;
}

export function dataURLtoFile(dataurl, filename) {
  const arr = dataurl.split(',');
  const mime = arr[0].match(/:(.*?);/)[1];
  const bstr = atob(arr[1]);
  let n = bstr.length;
  const u8arr = new Uint8Array(n)
;

  while (n--) {
    u8arr[n] = bstr.charCodeAt(n)
;
  }
  return new File([u8arr], filename, { type: mime });
}

export function arrayBufferToBase64(buffer) {
  let binary = '';
  const bytes = new Uint8Array(buffer);
  const len = bytes.byteLength;
  for (let i = 0; i < len; i++) {
    binary += String.fromCharCode(bytes[i]);
  }
  return btoa(binary);
}

export function saveBase64(name, base64) {
  const link = document.createElement('a');
  link.href = base64;
  const fileName = name;
  link.download = fileName;
  link.click();
}

export function saveByteArray(name, byte) {
  const blob = new Blob([byte], { type: mimeTypes.contentType(name) });
  const link = document.createElement('a');
  link.href = window.URL.createObjectURL(blob);
  const fileName = name;
  link.download = fileName;
  link.click();
}

export function cloneData(data) {
  if (!isEmpty(data)) {
    const str = JSON.stringify(data);
    if (!isEmpty(str)) {
      return JSON.parse(str);
    }
  }
  return null;
}

export function isNullOrUndefined(value) {
  return value === null || value === undefined;
}

export const isDev = 'development' === process.env.NODE_ENV;
export const isProduct = 'production' === process.env.NODE_ENV;
