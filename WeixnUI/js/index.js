wx.ready(function () {

function chooseUpload(selector) {
    
    wx.chooseImage({
        success: function (res) {
            $("#loading").show();
            $(function () {
                $.each(res.localIds, function (i, n) {
                    wx.uploadImage({
                        localId: res.localIds.toString(), // 需要上传的图片的本地ID，由chooseImage接口获得
                        isShowProgressTips: 0, // 默认为1，显示进度提示
                        success: function (res1) {
                            $.ajax({
                                url: 'http://www.focuswin.cn/wxapi/face/detect/' + res1.serverId,
                                dataType: "json",
                                success: function (data) {
                                    $("#loading").hide();
                                    if (JSON.parse(data).length == 1) {
                                        $(selector).html('<img src="' + n + '" /> <br />')
                                            .data('faceId', JSON.parse(data)[0].faceId);
                                    } else if (JSON.parse(data).length > 1) {
                                        alert('请选择单人照哦')
                                    } else {
                                        alert('啊，我看不到你的脸~')
                                    }
                                    
                                }
                            })

                        },
                        fail: function (res) {
                            alert(JSON.stringify(res));
                        }
                    });
                });
            });
        }
    });
}
document.querySelector('#parent1').onclick = function () {
    chooseUpload('#parent1')
      
  };

  document.querySelector('#parent2').onclick = function () {
      chooseUpload('#parent2')
  };

  document.querySelector('#child').onclick = function () {
      chooseUpload('#child')
  };


  function verify(selector, parent, child) {
      $("#loading").show();
      $.ajax({
          url: 'http://www.focuswin.cn/wxapi/face/verify/' + parent + '/' + child,
         
          dataType: "json",
          success: function (data) {
              $("#loading").hide();
              $(selector).html('相似度：' + (JSON.parse(data).confidence * 100).toFixed(2) + '%')
          }
      })
      
  }
  document.querySelector('#uploadImage').onclick = function () {
      var parent1Img = $('#parent1').data('faceId');
      var parent2Img = $('#parent2').data('faceId');
      var childImg = $('#child').data('faceId');
      if (parent1Img && parent2Img && childImg) {
          $("#loading").show();
          verify('.parent1like', parent1Img, childImg);
          verify('.parent2like', parent2Img, childImg);
      } else {
          alert('请选择父母及孩子的3张照片')
      }
      
     
  }
 

 



  var shareData = {
      title: '测测孩子跟谁像',
      desc: '来看看孩子跟爸爸比较像还是跟妈妈比较像',
      link: window.location.href,
      imgUrl: 'http://www.focuswin.cn/WeFace/fonts/family.jpg'
  };
  wx.onMenuShareAppMessage(shareData);
  wx.onMenuShareTimeline(shareData);

  function decryptCode(code, callback) {
    $.getJSON('/jssdk/decrypt_code.php?code=' + encodeURI(code), function (res) {
      if (res.errcode == 0) {
        codes.push(res.code);
      }
    });
  }
});

wx.error(function (res) {
  alert(res.errMsg);
});

