import $ from "jquery";

export default class req {
  getAjax(url: string, callback: any) {
    $.ajax({
      contentType: "application/json; charset=utf-8",
      dataType: "json",
      url: url,
      method: "Get",
      success: callback,
      error: callback
    });
  }

  postAjax(url: string, callback: any) {
    $.ajax({
      contentType: "application/json; charset=utf-8",
      dataType: "json",
      url: url,
      method: "POST",
      success: callback,
      error: callback
    });
  }

  putAjax() {}

  deleteAjax() {}

  getXMLHttp(url: string, callback: any) {
    var xmlhttp = new XMLHttpRequest();
    xmlhttp.open("GET", url, true);
    xmlhttp.onreadystatechange = callback;
    xmlhttp.send(null);
  }

  genToken(username: string) {
    var result = "";
    var characters =
      "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
    var charactersLength = characters.length;
    for (var i = 0; i < 10; i++) {
      result += characters.charAt(Math.floor(Math.random() * charactersLength));
    }
    return result;
  }
}
