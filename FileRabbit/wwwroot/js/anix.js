/*
	__________________________________________________________________
	AniX 3.2

	Автор: WeRtOG
*/

$.fn.isInViewport = function() {
    var top_of_element = $(this).offset().top;
    var bottom_of_element = $(this).offset().top + $(this).outerHeight();
    var bottom_of_screen = $(window).scrollTop() + window.innerHeight;
    var top_of_screen = $(window).scrollTop();

    if((bottom_of_screen > top_of_element) && (top_of_screen < bottom_of_element)){
        return true;
    }
    else {
       return false;
    }
};

var anix_default_hold = 0; //Заддержка по умолчанию
var anix_default_speed = 500; //Скорость по умолчанию
var prev_anix_speed = 0; //Предыдущая скорость(изначально 0)

var anix_default_left = "-=100";  //Сдвиг влево(изначально 50рх)
var anix_default_left_dis = "+=100"; //Возвращение из сдвига влево(изначально 50рх)
var anix_default_up = 0; //Сдвиг вверх(изначально 0)
var anix_default_up_dis = 0; //Сдвиг вверх(изначально 0)

function anix_scr_logic($el) {
	$el.each(function(i) {
		if($(this).attr("data-wait") == undefined)
			$(this).attr("data-wait", "true");
		if($(this).isInViewport() && $(this).attr("data-wait") != 'false') {
			$(this).removeAttr("data-after-scroll");
			anix_do($(this));
			$(this).attr("data-wait", "false");
		}
	});
}
function anix_scr($el) {
	$(window).scroll(function() {
		anix_scr_logic($el);
	});
	anix_scr_logic($el);
}
function anix_do($el) { //Сама функция для анимации
	var xhrreq = new XMLHttpRequest();
	xhrreq.open('GET', window.location, false);
	xhrreq.send(null);
	if(xhrreq.getResponseHeader('AniX-Disable') == null) {
		$el.each(function(i) { //Функция для каждого элемента
			$(this).css("opacity", 0); //Ещё раз ставим полную прозрачность(на всякий)
			if($(this).attr("data-after-scroll") == undefined) {
				if($(this).attr("data-speed") != undefined) { //Если указана пользовательская скорость
					anix_speed = Number($(this).attr("data-speed"));
				} else { //Иначе
					anix_speed = anix_default_speed;
				}
				if($(this).attr("data-hold") != undefined) { //Если указана пользовательская заддержка
					anix_hold = Number($(this).attr("data-hold"));
				} else { //Иначе
					anix_hold = anix_default_hold;
				}
				if($(this).attr("data-continue") == "true") { //Если указано начинать анимацию только по завершении предыдущей
					anix_delay = ((i++) * anix_hold)-(-prev_anix_speed);
				} else {
					anix_delay = anix_hold;
				}
				switch($(this).attr("data-direction")) { //Направление
					default:
						anix_left = anix_default_left;
						anix_left_dis = anix_default_left_dis;
						anix_up = anix_default_up;
						anix_up_dis = anix_default_up_dis;
						break;
					case "left":
						anix_left = "-=100";
						anix_left_dis = "+=100";
						anix_up = 0;
						anix_up_dis = 0;
						break;
					case "right":
						anix_left = "+=100";
						anix_left_dis = "-=100";
						anix_up = 0;
						break;
					case "up":
						anix_up = "-=100";
						anix_up_dis = "+=100";
						anix_left = 0;
						anix_left_dis = 0;
						break;
					case "down":
						anix_up = "+=100";
						anix_up_dis = "-=100";
						anix_left = 0;
						anix_left_dis = 0;
						break;
				}
				switch($(this).attr("data-fx")) { //Сами эффекты
					default:
                        try {
                            $(this).delay(anix_delay).transition({ opacity: 1}, anix_speed);
                        }
                        catch (err) {
                            $(this).transition({ opacity: 1}, anix_speed);
                        }
						break;
					case "zoom":
						$(this).transition({ scale: 0.3}, 0);
						$(this).delay(anix_delay).transition({ opacity: 1, scale: 1}, anix_speed, 'cubic-bezier(0.785, 0.135, 0.15, 0.86)');
						break;
					case "move":
						$(this).transition({ x: anix_left, y: anix_up}, 0);
						$(this).delay(anix_delay).transition({ opacity: 1, x: anix_left_dis, y: anix_up_dis}, anix_speed, 'cubic-bezier(0.785, 0.135, 0.15, 0.86)');
						break;
					case "puff":
						$(this).transition({ scale: 1.3}, 0);
						$(this).delay(anix_delay).transition({ opacity: 1, scale: 1}, anix_speed, 'cubic-bezier(0.785, 0.135, 0.15, 0.86)');
						break;
				}
				prev_anix_speed = anix_speed; //Указываем текущую скорость как предыдущую
			}
		});
	} else {
		$el.css("opacity", "1");
	}
}
function getBrowser(){
	var browserName;

	// Opera 8.0+
	if ((window.opr && opr.addons) || window.opera || (navigator.userAgent.indexOf(' OPR/') >= 0))
	browserName = "Opera";

	// Firefox 1.0+
	if (typeof InstallTrigger !== 'undefined')
	browserName = "Firefox";

	// At least Safari 3+: "[object HTMLElementConstructor]"
	if (Object.prototype.toString.call(window.HTMLElement).indexOf('Constructor') > 0)
	browserName = "Safari";

	// Internet Explorer 6-11
	if ((/*@cc_on!@*/false) || (document.documentMode))
	browserName = "IE";

	// Edge 20+
	if (!(document.documentMode) && window.StyleMedia)
	browserName = "Edge";

	// Chrome 1+
	if (window.chrome && window.chrome.webstore)
	browserName = "Chrome";

	return browserName;
}
$(document).ready(function() {
    var browser = getBrowser();
	var $anix = $('.anix'); //Элемент
    var $anix_scr = $('.anix[data-after-scroll=true]');
    $anix.css("opacity", 0); //Ставим полную прозрачность

    anix_do($anix); //Запускаем функцию
    if(browser != 'Firefox')
        anix_scr($anix_scr);
    else
        $anix_scr.css("opacity", 1);

	$(".anix-replay").click(function() { //Функция кнопки перезапуска анимации
		$anix.css("opacity", 0);
		$anix.removeAttr("data-wait");
		anix_do($anix);
		anix_scr($anix_scr);
	});
});
